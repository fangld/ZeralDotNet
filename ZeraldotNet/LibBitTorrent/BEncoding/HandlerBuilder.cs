using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class HandlerBuilder
    {
        private DictNode rootNode;
        private DictNode currentNode;

        public HandlerBuilder(DictNode rootHandler)
        {
            this.rootNode = rootHandler;
            currentNode = rootNode;
        }

        public void AddChild(BytesNode key, BEncodedNode value)
        {
            DictNode parentNode = currentNode;
            currentNode = new DictNode(key, value);
            parentNode.Add(key, value);
        }

        public void AddTo(DictNode parentNode, BytesNode key, BEncodedNode value)
        {
            parentNode.Add(key, value);
        }

        public override string ToString()
        {
            return rootNode.ToString();
        }
    }
}
