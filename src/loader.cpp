#include <combaseapi.h>
#include "loader.h"
#include "path_info.h"
#include "mapsharing.h"
#include "mapdata.h"
#include "mapbuffer.h"
#include "overmap.h"
#include "submap.h"
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

    void loadSaveGame(char* worldName, char* saveName) {
        WORLDPTR world = world_generator->get_world(worldName);
        world_generator->set_active_world(world);
        try {
            g->setup();
        }
        catch (const std::exception &err) {
            debugmsg("Error: %s", err.what());
            g->u = player();
        }

        std::vector<save_t>::iterator it;
        for (it = world->world_saves.begin(); it < world->world_saves.end(); it++) {
            if (it->player_name() == saveName) {
                g->load(worldName, *it);
                break;
            }
        }
    }

    Entity* getEntities(IVector2 from, IVector2 to) {
        IVector2 bottomLeft;
        IVector2 topRight;
        if (from.x < to.x && from.y < to.y) {
            bottomLeft = from;
            topRight = to;
        }
        else {
            bottomLeft = to;
            topRight = from;
        }

        int i = 0;
        std::vector<Entity> ent;
        Entity* entities;
        for (int dx = bottomLeft.x; dx <= topRight.x; dx++) {
            for (int dy = bottomLeft.y; dy <= topRight.y; dy++) {
                tripoint p = tripoint(dx, dy, 0);
                Creature* crit = g->critter_at(p, true);
                if (crit->is_player()) continue;
                sm_to_om(p);
                Entity e;
                e.hp = crit->get_hp();
                e.maxHp = crit->get_hp_max();
                e.isMonster = crit->is_monster();
                e.isNpc = crit->is_npc();
                Creature::Attitude attitude = crit->attitude_to(g->u);
                std::string strAtt = "";
                if (attitude == Creature::Attitude::A_FRIENDLY) {
                    e.attitude = "friendly";
                }
                else if (attitude == Creature::Attitude::A_HOSTILE) {
                    e.attitude = "hostile";
                }
                else if (attitude == Creature::Attitude::A_NEUTRAL) {
                    e.attitude = "neutral";
                }
                else {
                    e.attitude = "any";
                }
                ent.push_back(e);
            }
        }
        entities = (Entity*)::CoTaskMemAlloc(ent.size() * sizeof(Entity));
        for (i = 0; i <= ent.size(); i++)
        {
            entities[i] = ent.at(i);
        }
        return entities;
    }

    IVector3 playerPos()
    {
        tripoint pos = g->u.global_square_location();
        IVector3 res = { pos.x, pos.y, pos.z };
        return res;
    }

    Map* getTilesBetween(IVector2 from, IVector2 to) {
        Map* map = (Map*)::CoTaskMemAlloc(sizeof(Map));
        IVector2 bottomLeft;
        IVector2 topRight;
        if (from.x < to.x && from.y < to.y) {
            bottomLeft = from;
            topRight = to;
        }
        else {
            bottomLeft = to;
            topRight = from;
        }

        int width = topRight.x - bottomLeft.x + 1;
        int height = topRight.y - bottomLeft.y + 1;
        map->width = width;
        map->height = height;
        map->tiles = (Tile*)::CoTaskMemAlloc(sizeof(Tile) * width * height);

        int z = 0;
        submap *sm;
        IVector2 submapFrom = { bottomLeft.x / SEEX, bottomLeft.y / SEEY };
        IVector2 submapTo = { topRight.x / SEEX, topRight.y / SEEY };

        IVector2 submapFromS = { bottomLeft.x % SEEX, bottomLeft.y % SEEY };
        IVector2 submapToS = { topRight.x % SEEX, topRight.y % SEEY };

        IVector3 ppos = playerPos();

        for (int x = submapFrom.x; x <= submapTo.x; x++) {
            for (int y = submapFrom.y; y <= submapTo.y; y++) {
                sm = MAPBUFFER.lookup_submap(x, y, z);
                if (sm == nullptr) {
                    sm = g->m.generateSubmap(x, y, z);
                }
                IVector2 sfrom = { 0,0 };
                IVector2 sto = { SEEX - 1, SEEY - 1};
              
                if (x == submapFrom.x) {
                    sfrom.x = submapFromS.x;
                }

                if (y == submapFrom.y) {
                    sfrom.y = submapFromS.y;
                }

                if (x == submapTo.x) {
                    sto.x = submapToS.x;
                }

                if (y == submapTo.y) {
                    sto.y = submapToS.y;
                }

                for (int sx = sfrom.x; sx <= sto.x; sx++) {
                    for (int sy = sfrom.y; sy <= sto.y; sy++) {
                        int i = (y * SEEY + sy - from.y) * width + x * SEEX + sx - from.x;
                        map->tiles[i].loc.x = x * SEEX + sx;
                        map->tiles[i].loc.y = z;
                        map->tiles[i].loc.z = y * SEEY + sy; // swap z and y for unity coordinate system

                        IVector2 localPos = { ppos.x - map->tiles[i].loc.x, 
                                              ppos.y - map->tiles[i].loc.y };
                        
                        map->tiles[i].ter = sm->get_ter(sx, sy);
                        map->tiles[i].furn = sm->get_furn(sx, sy);
                        map->tiles[i].seen = false;
                        if (localPos.x >= 0 && localPos.y >= 0 &&
                            localPos.x < 120 && localPos.y < 120) {
                            map->tiles[i].seen = g->u.sees(localPos.x, localPos.y);
                        }
                    }
                }
            }
        }

        return map;
    }

    int terId(char* str) {
        bool prevTestMode = test_mode;
        test_mode = true;
        ter_id id(str);
        test_mode = prevTestMode;
        return id;
    }

    GameData* getGameData(void) {
        GameData* data = (GameData*)::CoTaskMemAlloc(sizeof(GameData));

        w_point* w = g->weather_precise.get();
        weather_generator gen = g->get_cur_weather_gen();
        const tripoint ppos = g->u.pos();

        int width = 10, height = 10;

        data->playerPosition.x = ppos.x;
        data->playerPosition.y = ppos.z;
        data->playerPosition.z = ppos.y; // swap z and y for unity coordinate system... should we really do this here?

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
                data->map.tiles[i].ter = g->m.ter(p);
                data->map.tiles[i].furn = g->m.furn(p);
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
    
    void deinit(void) {
        deinitDebug();
        if (g != NULL) {
            delete g;
        }
        DynamicDataLoader::get_instance().unload_data();
        endwin();
    }
}