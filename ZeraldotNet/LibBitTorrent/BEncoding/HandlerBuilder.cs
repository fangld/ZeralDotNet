using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
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

        public void AddChild(BytestringHandler key, Handler value)
        {
            DictionaryHandler parentNode = currentNode;
            currentNode = new DictionaryHandler(key, value);
            parentNode.Add(key, value);
        }

        public void AddTo(DictionaryHandler parentNode, BytestringHandler key, Handler value)
        {
            parentNode.Add(key, value);
        }

        public override string ToString()
        {
            return rootNode.ToString();
        }
    }
}
