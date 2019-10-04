using System;

namespace CookieMonster
{
    public class CookieValue
    {
        public string Browser { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }

        public DateTime LastAccess { get; set; }
        public DateTime Expires { get; set; }

        public bool IsValid
        {
            get
            {
                // sanity check. datetimes are stored weird in each cookie type. make sure i haven't converted these incredibly wrong.
                // some early conversion attempts produced years like 42, 1955, 4033
                var _5yearsPast = DateTime.UtcNow.AddYears(-5);
                if (LastAccess < _5yearsPast || LastAccess > DateTime.UtcNow)
                    return false;
                // don't check expiry. some sites are setting stupid values for year. eg: 9999
                return true;
            }
        }

        public bool HasExpired => Expires < DateTime.UtcNow;
    }
}
