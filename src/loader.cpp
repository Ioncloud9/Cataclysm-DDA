#include <combaseapi.h>
#include "loader.h"
#include "path_info.h"
#include "mapsharing.h"
#include "mapdata.h"
#include "debug.h"
#include "options.h"
#include "translations.h"
#include "game.h"
#include "worldfactory.h"
#include "filesystem.h"
#include "player.h"
#include "map.h"
#include "input.h"
#include "main_menu.h"
#include "weather_gen.h"
#include "init.h"

extern bool assure_dir_exist(std::string const &path);
extern void exit_handler(int s);
extern bool test_dirty;
extern input_context get_default_mode_input_context();

extern "C" {
    void init(bool openMainMenu) {
        //test_mode = true;
        // tyomalu: most code from original main.cpp but we ignore command line arguments
        // Set default file paths
        int seed = time(NULL);
        bool verifyexit = false;
        bool check_mods = false;
        std::string dump;
        dump_mode dmode = dump_mode::TSV;
        std::vector<std::string> opts;
        std::string world; /** if set try to load first save in this world on startup */
#ifdef PREFIX
#define Q(STR) #STR
#define QUOTE(STR) Q(STR)
        PATH_INFO::init_base_path(std::string(QUOTE(PREFIX)));
#else
        PATH_INFO::init_base_path("../");
#endif

#if (defined USE_HOME_DIR || defined USE_XDG_DIR)
        PATH_INFO::init_user_dir();
#else
        PATH_INFO::init_user_dir("../");
#endif
        PATH_INFO::set_standard_filenames();
        MAP_SHARING::setDefaults();

        if (!assure_dir_exist(FILENAMES["user_dir"].c_str())) {
            printf("Can't open or create %s. Check permissions.\n",
                FILENAMES["user_dir"].c_str());
            exit(1);
        }

        setupDebug();

        if (setlocale(LC_ALL, "") == NULL) {
            DebugLog(D_WARNING, D_MAIN) << "Error while setlocale(LC_ALL, '').";
        }

        // Options strings loaded with system locale. Even though set_language calls these, we
        // need to call them from here too.
        get_options().init();
        get_options().load();

        if (initscr() == nullptr) { // Initialize ncurses
            DebugLog(D_ERROR, DC_ALL) << "initscr failed!";
            return;
        }
        init_interface();
        noecho();  // Don't echo keypresses
        cbreak();  // C-style breaks (e.g. ^C to SIGINT)
        keypad(stdscr, true); // Numpad is numbers

        set_language();

        // skip curses initialization

        srand(seed);
        g = new game;

        // First load and initialize everything that does not
        // depend on the mods.
        try {
            g->load_static_data();
            if (verifyexit) {
                exit_handler(0);
            }
            if (!dump.empty()) {
                init_colors();
                exit(g->dump_stats(dump, dmode, opts) ? 0 : 1);
            }
            if (check_mods) {
                init_colors();
                exit(g->check_mod_data(opts) && !test_dirty ? 0 : 1);
            }
        }
        catch (const std::exception &err) {
            debugmsg("%s", err.what());
            exit_handler(-999);
        }

        g->init_ui();
        curs_set(0); // Invisible cursor here, because MAPBUFFER.load() is crash-prone

        if (openMainMenu) {
            main_menu menu;
            if (!menu.opening_screen()) {
                return;
            }
        }
        else {
            // this stuff is from main_menu.opening_screen(), need to be initialized
            // if we don't open menu
            world_generator->set_active_world(NULL);
            world_generator->init();

            if (!assure_dir_exist(FILENAMES["config_dir"])) {
                printf("Unable to make config directory. Check permissions.");
                return;
            }

            if (!assure_dir_exist(FILENAMES["savedir"])) {
                printf("Unable to make save directory. Check permissions.");
                return;
            }

            if (!assure_dir_exist(FILENAMES["templatedir"])) {
                printf("Unable to make templates directory. Check permissions.");
                return;
            }

            g->u = player();
        }
    }

    CStringArray* getWorldNames(void) {
        CStringArray* arr = (CStringArray*)::CoTaskMemAlloc(sizeof(CStringArray));
        const auto worlds = world_generator->all_worldnames();
        arr->len = worlds.size();

        if (worlds.empty()) {
            return arr;
        }

        // saved games are available
        size_t arrSize = sizeof(char*) * arr->len;
        arr->stringArray = (char**)::CoTaskMemAlloc(arrSize);
        memset(arr->stringArray, 0, arrSize);

        for (int i = 0; i < worlds.size(); i++) {
            arr->stringArray[i] = (char*)::CoTaskMemAlloc(worlds[i].length() + 1);
            strcpy(arr->stringArray[i], worlds[i].c_str());
        }
        return arr;
    }

    CStringArray* getWorldSaves(char *worldName) {
        CStringArray* arr = (CStringArray*)::CoTaskMemAlloc(sizeof(CStringArray));
        const auto saves = world_generator->get_world(worldName)->world_saves;
        arr->len = saves.size();

        if (saves.empty()) {
            return arr;
        }        

        size_t arrSize = sizeof(char*) * arr->len;
        arr->stringArray = (char**)::CoTaskMemAlloc(arrSize);
        memset(arr->stringArray, 0, arrSize);

        for (int i = 0; i < saves.size(); i++) {
            arr->stringArray[i] = (char*)::CoTaskMemAlloc(saves[i].player_name().length() + 1);
            strcpy(arr->stringArray[i], saves[i].player_name().c_str());
        }
        return arr;
    }

    void loadGame(char* worldName) {
        WORLDPTR world = world_generator->get_world(worldName);
        world_generator->set_active_world(world);
        try {
            g->setup();
        }
        catch (const std::exception &err) {
            debugmsg("Error: %s", err.what());
            g->u = player();
        }
        //auto save = world->world_saves[saveGame];
        g->load(world->world_name);
    }

    GameData* getGameData(void) {
        GameData* data = (GameData*)::CoTaskMemAlloc(sizeof(GameData));

        w_point* w = g->weather_precise.get();
        weather_generator gen = g->get_cur_weather_gen();
        const tripoint ppos = g->u.pos();

        int width = 10, height = 10;

        data->calendar.season = calendar::turn.get_season();
        data->calendar.time = (char*)::CoTaskMemAlloc(calendar::turn.print_time().length() + 1);
        strcpy(data->calendar.time, calendar::turn.print_time().c_str());
        data->calendar.years = calendar::turn.years();
        data->calendar.days = calendar::turn.days();
        data->calendar.moon = calendar::turn.moon();
        data->calendar.isNight = calendar::turn.is_night();

        data->weather.type = gen.get_weather_conditions(*w);
        data->weather.temperature = w->temperature;
        data->weather.humidity = w->humidity;
        data->weather.wind = w->windpower;
        data->weather.pressure = w->pressure;
        data->weather.acidic = w->acidic;

        data->map.width = width;
        data->map.height = height;
        data->map.tiles = (Tile*)::CoTaskMemAlloc(sizeof(Tile) * width * height);

        int i = 0;
        for (int dx = -width / 2; dx < width / 2; dx++) {
            for (int dy = -height / 2; dy < height / 2; dy++) {
                const tripoint p = ppos + tripoint(dx, dy, 0);
                ter_str_id ter = g->m.ter(p)->id;
                data->map.tiles[i].ter = (char*)::CoTaskMemAlloc(ter.str().length() + 1);
                strcpy(data->map.tiles[i].ter, ter.c_str());
                furn_str_id furn = g->m.furn(p)->id;
                data->map.tiles[i].furn = (char*)::CoTaskMemAlloc(furn.str().length() + 1);
                strcpy(data->map.tiles[i].furn, furn.c_str());
                data->map.tiles[i].loc.x = p.x;
                data->map.tiles[i].loc.y = p.z;
                data->map.tiles[i].loc.z = p.y; // swap z and y for unity coordinate system
                i++;
            }
        }
        return data;
    }

    void doAction(char* action) {
        g->extAction = action;
        g->do_turn();
    }

    void doTurn(void) {
        g->do_turn();
    }

    int getTurn(void) {
        return calendar::turn.get_turn();
    }
    
    IVector3* playerPos(void) {
        IVector3* pVec = (IVector3*)::CoTaskMemAlloc(sizeof(IVector3));
        pVec->x = g->u.posx();
        pVec->y = g->u.posy();
        pVec->z = g->u.posz();
        return pVec;
    }


    void deinit(void) {
        deinitDebug();
        if (g != NULL) {
            delete g;
        }
        DynamicDataLoader::get_instance().unload_data();
        endwin();
    }
}