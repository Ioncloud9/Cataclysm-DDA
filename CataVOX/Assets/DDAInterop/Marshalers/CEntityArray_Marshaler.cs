using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Assets.DDAInterop.Marshalers
{
    public class CEntityArray_Marshaler : ICustomMarshaler
    {
        static ICustomMarshaler GetInstance(string cookie)
        {
            return new CEntityArray_Marshaler();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            CEntityArray arr = (CEntityArray)Marshal.PtrToStructure(pNativeData, typeof(CEntityArray));

            Entity[] managedEntityArray = new Entity[arr.size];

            for (int i = 0; i < arr.size; i++)
            {
                IntPtr p = new IntPtr(arr.entityArray.ToInt64() + i * Marshal.SizeOf(typeof(CEntity)));
                CEntity entity = (CEntity)Marshal.PtrToStructure(p, typeof(CEntity));

                managedEntityArray[i] = new Entity();
                managedEntityArray[i].hp = entity.hp;
                managedEntityArray[i].isMonster = entity.isMonster != 0;
                managedEntityArray[i].isNpc = entity.isNpc != 0;
                managedEntityArray[i].loc = entity.loc;
                managedEntityArray[i].maxHp = entity.maxHp;
                managedEntityArray[i].type = entity.type;
                managedEntityArray[i].attitude = (Attitude)entity.attitude;
            }

            return managedEntityArray;
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            CEntityArray arr = (CEntityArray)Marshal.PtrToStructure(pNativeData, typeof(CEntityArray));
            Marshal.FreeCoTaskMem(arr.entityArray);
            Marshal.FreeCoTaskMem(pNativeData);
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            // don't send entities arrays back to c++ yet
        }

        public int GetNativeDataSize()
        {
            return Marshal.SizeOf(typeof(CStringArray));
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            // don't send string arrays to c++ yet
            return IntPtr.Zero;
        }
    }
}
