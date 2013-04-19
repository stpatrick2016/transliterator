
namespace Transliterator.LayoutEditor.Infrastructure
{
    public class MappingPair
    {
        public MappingPair()
        {
            IsUpperCaseAutomatic = true;
        }

        #region Properties
        public string Source { get; set; }
        public string LowerCaseTarget { get; set; }
        public string UpperCaseTarget { get; set; }

        public bool IsUpperCaseAutomatic { get; set; }
        #endregion
    }
}
