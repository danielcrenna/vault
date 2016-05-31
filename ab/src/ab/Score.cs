using System.Collections.Generic;

namespace ab
{
    public class Score
    {
        public IList<Alternative> Alternatives { get; private set; }
        public Alternative Best { get; set; }
        public Alternative Choice { get; set; }
        public Alternative Base { get; set; }
        public Alternative Least { get; set; }

        public Score()
        {
            Alternatives = new List<Alternative>();
        }
    }
}