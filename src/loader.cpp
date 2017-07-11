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
#include "calendar.h"

extern bool assure_dir_exist(std::string const &path);
extern void exit_handler(int s);
extern bool test_dirty;
extern input_context get_default_mode_input_context();

extern "C" {
    void init(void) {
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


        // from main_menu.cpp
        main_menu menu;
        if (!menu.opening_screen()) {
            return;
        }
    /*    world_generator->set_active_world(NULL);
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

        g->u = player();*/

    }

    void getWorldNames(/*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver) {
        const auto worlds = world_generator->all_worldnames();

        if (worlds.empty()) {
            *stringsCountReceiver = 0;
            return;
        }

        // saved games are available
        *stringsCountReceiver = worlds.size();
        if (worlds.size() > 0) {
            size_t arraySize = sizeof(char*) * (*stringsCountReceiver);

            *stringBufferReceiver = (char**)::CoTaskMemAlloc(arraySize);
            memset(*stringBufferReceiver, 0, arraySize);

            for (int i = 0; i < worlds.size(); i++) {
                (*stringBufferReceiver)[i] = (char*)::CoTaskMemAlloc(worlds[i].length() + 1);
                strcpy((*stringBufferReceiver)[i], worlds[i].c_str());
            }
        }
    }

    void getWorldSaves(char *worldName, /*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver) {
        std::string name = std::string(worldName);
        *stringsCountReceiver = 0;

        const auto saves = world_generator->get_world(name)->world_saves;
        if (saves.empty()) {
            *stringsCountReceiver = 0;
            return;
        }        

        *stringsCountReceiver = saves.size();
        size_t arraySize = sizeof(char*) * saves.size();
        *stringBufferReceiver = (char**)::CoTaskMemAlloc(arraySize);
        memset(*stringBufferReceiver, 0, arraySize);

        for (int i = 0; i < saves.size(); i++) {
            (*stringBufferReceiver)[i] = (char*)::CoTaskMemAlloc(saves[i].player_name().length() + 1);
            strcpy((*stringBufferReceiver)[i], saves[i].player_name().c_str());
        }
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

    void getTer(/*out*/ char** ter) {
        const tripoint ppos = g->u.pos();

        ter_str_id t = g->m.ter(ppos)->id;

        *ter = (char*)::CoTaskMemAlloc(t.str().length() + 1);
        strcpy(*ter, t.c_str());
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
    
    int playerX(void) {
        return g->u.posx();
    }

    int playerY(void) {
        return g->u.posy();
    }

    void deinit(void) {
        deinitDebug();
        if (g != NULL) {
            delete g;
        }
        //Mix_CloseAudio();
    }
}