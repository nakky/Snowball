using System;
using System.Collections.Generic;

namespace Snowball
{
    public class ComGroup
    {
        public string Name { get; private set; }

        internal List<ComNode> NodeList { get; private set; }

        public ComGroup(string name)
        {
            Name = name;
            NodeList = new List<ComNode>();
        }

        public void Add(ComNode node) { NodeList.Add(node); }
        public void Remove(ComNode node) { NodeList.Remove(node); }
    }
}
