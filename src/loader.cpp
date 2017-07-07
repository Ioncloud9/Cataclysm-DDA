#include <combaseapi.h>
#include "loader.h"
#include "path_info.h"
#include "mapsharing.h"
#include "debug.h"
#include "options.h"
#include "translations.h"
#include "game.h"
#include "worldfactory.h"
#include "filesystem.h"
#include "player.h"

extern bool assure_dir_exist(std::string const &path);
extern void exit_handler(int s);
extern bool test_dirty;

extern "C" {
    void init(void) {
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

        if (setlocale(LC_ALL, "") == NULL) {
            DebugLog(D_WARNING, D_MAIN) << "Error while setlocale(LC_ALL, '').";
        }

        // Options strings loaded with system locale. Even though set_language calls these, we
        // need to call them from here too.
        get_options().init();
        get_options().load();
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
                exit(g->dump_stats(dump, dmode, opts) ? 0 : 1);
            }
            if (check_mods) {
                exit(g->check_mod_data(opts) && !test_dirty ? 0 : 1);
            }
        }
        catch (const std::exception &err) {
            debugmsg("%s", err.what());
            exit_handler(-999);
        }

        // from main_menu.cpp

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

    const void getWorldNames(/*[out]*/ char*** stringBufferReceiver, /*[out]*/ int* stringsCountReceiver) {
        if (world_generator->all_worldnames().empty()) {
            *stringsCountReceiver = 0;
            return;
        }

        // saved games are available
        *stringsCountReceiver = world_generator->all_worldnames().size();
        size_t arraySize = sizeof(char*) * (*stringsCountReceiver);

        *stringBufferReceiver = (char**)::CoTaskMemAlloc(arraySize);
        memset(*stringBufferReceiver, 0, arraySize);
        
        const auto worlds = world_generator->all_worldnames();

        for (int i = 0; i < world_generator->all_worldnames().size(); i++) {
            (*stringBufferReceiver)[i] = (char*)::CoTaskMemAlloc(worlds[i].length() + 1);
            strcpy((*stringBufferReceiver)[i], worlds[i].c_str());
        }
        return;
    }
}