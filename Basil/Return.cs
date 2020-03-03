using System;

namespace BasilLang
{
    public class Return : Exception
    {
        public readonly object value;

        public Return(object value) {
            //super(null, null, false, false);
            this.value = value;
        }
    }
}
