using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apim.WebClient.Models
{
    public class AccessToken
    {
        //public string Expires { get; set; }
        public string access_token { get; set; }
        //public string expires { get; set; }
        public int  expires_in { get; set; }

        public string  refresh_expires_in { get; set; }

        public string  refresh_token { get; set; }
        public string  token_type { get; set; }

        public string  id_token { get; set; }   

        public string  session_state { get; set; }
         public string  scope { get; set; }
    

        public AccessToken()
        {
           
            

        }

    }
}
