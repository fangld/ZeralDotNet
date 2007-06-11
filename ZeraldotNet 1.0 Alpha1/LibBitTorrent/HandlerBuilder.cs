using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class HandlerBuilder
    {
        private DictionaryHandler rootNode;
        private DictionaryHandler currentNode;

        public HandlerBuilder(DictionaryHandler rootHandler)
        {
            this.rootNode = rootHandler;
            currentNode = rootNode;
        }

        public void AddChild(ByteArrayHandler key, Handler value)
        {
            DictionaryHandler parentNode = currentNode;
            currentNode = new DictionaryHandler(key, value);
            parentNode.Add(key, value);
        }

        public void AddTo(DictionaryHandler parentNode, ByteArrayHandler key, Handler value)
        {
            parentNode.Add(key, value);
        }

        public override string ToString()
        {
            return rootNode.ToString();
        }
    }
}
