using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class zMQResponse
    {
        public zMQResponse() { }

        public zMQResponse(string response)
        {
            zMQResponse res;
            if (!TryParse(response, out res)) return;
            ID = res.ID;
            Data = res.Data;
        }
        public zMQResponse(Guid id, string data = null)
        {
            ID = id;
            Data = data;
        }

        public Guid ID { get; set; }
        public string Data { get; set; }


        public static zMQResponse Parse(string response)
        {
            var ret = new zMQResponse();
            var split = response.Split(voxConst.ZMQ_COMMAND_DELIM).ToList();
            ret.ID = new Guid(split[0]);
            split.RemoveAt(0);
            ret.Data = string.Join(voxConst.ZMQ_COMMAND_DELIM.ToString(), split.ToArray());
            return ret;
        }

        public static bool TryParse(string s, out zMQResponse result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", ID, Data);
        }

    }
}
