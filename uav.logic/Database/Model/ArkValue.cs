using System;

namespace uav.logic.Database.Model
{
    public class ArkValue
    {
        public ulong user_id { get; set; }
        public double Gv { get; set; }
        public int Base_Credits { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool Oopsed { get; set; }
    }
}