using System;
using System.Collections.Generic;
using System.IO;
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
			var allBlockTypes = ass.GetTypes().Where(x => !x.IsAbstract && x.GetInterfaces().Contains(typeof(IBlock)));

            foreach (var type in allBlockTypes)
            {
                var attrib = (BlockTypeAttribute[])type.GetCustomAttributes(typeof(BlockTypeAttribute), true);
                if (attrib.Length == 0) continue;
                BlockMap.Add(attrib[0].Type, (IBlock)Activator.CreateInstance(type));
            }

            //Do something with CustomBlock... maybe load files from json to create CustomBlocks and add to BlockMap
            var files = Directory.GetFiles("Assets/tiles", "t_*.vox");
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file).ToLower();
                var block = new CustomBlock
                {
                    Type = name
                };
                BlockMap.Add(name, block);
            }
        }

        public static IBlock CreateBlock(string type, IVector3 location, IChunk parent)
        {
            IBlock t;
            if (!BlockMap.TryGetValue(type, out t)) t = BlockMap["t_unknown"];
            return t.Create(type, location, parent);
        }
    }
}
