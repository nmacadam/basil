using System;

namespace BasilLang
{
    public class RuntimeError : Exception//RuntimeException
    {
        public readonly Token token;

        public RuntimeError(Token token, string message) {
            //super(message);
            //base(message);
            this.token = token;
        }
    }
}
