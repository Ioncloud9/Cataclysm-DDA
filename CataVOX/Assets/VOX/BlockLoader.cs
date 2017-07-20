using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.Framework;
using Assets.VOX.Blocks;

namespace Assets.VOX
{
    public static class BlockLoader
    {
        public static Dictionary<string, IBlock> BlockMap = new Dictionary<string, IBlock>();

        static BlockLoader()
        {
            var ass = Assembly.GetExecutingAssembly();
            foreach (var type in ass.GetTypes().Where(x => !x.IsAbstract && x.GetInterfaces().Contains(typeof(IBlock))))
            {
                var attrib = (BlockTypeAttribute[])type.GetCustomAttributes(typeof(BlockTypeAttribute), true);
                if (attrib.Length == 0) continue;
                BlockMap.Add(attrib[0].Type, (IBlock)Activator.CreateInstance(type));
            }

            //Do something with CustomBlock... maybe load files from json to create CustomBlocks and add to BlockMap
            foreach (var t in blockTypes)
            {
                BlockMap.Add(t, new CustomBlock());
            }
        }

        public static IBlock CreateBlock(string type, IVector3 location, IChunk parent)
        {
            IBlock t;
            if (!BlockMap.TryGetValue(type, out t)) t = BlockMap["t_unknown"];
            return t.Create(type, location, parent);
        }

        public static List<string> blockTypes = new List<string>()
        {
            //"t_Dirt", already a block
            "t_console",
            "t_console_broken",
            "t_curtains",
            "t_dirt",
            "t_door_c",
            "t_door_o",
            "t_fence_h",
            "t_fence_v",
            "t_floor",
            "t_grass",
            "t_pavement",
            "t_pavement_y",
            "t_player",
            "t_rock",
            "t_rock_floor",
            "t_shrub",
            "t_sidewalk",
            "t_stairs_down",
            "t_stairs_up",
            "t_unknown",
            "t_unseen",
            "t_wall",
            "t_water_dp",
            "t_water_sh"
        };
    }
}
