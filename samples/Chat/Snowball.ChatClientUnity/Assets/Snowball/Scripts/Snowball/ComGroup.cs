using System;
using System.Collections.Generic;

namespace Snowball
{
    public class ComGroup : IEnumerable<ComNode>
    {
        public string Name { get; private set; }

        public List<ComNode> NodeList { get; private set; }
        public Dictionary<string, ComNode> IpNodeMap { get; private set; }

        public IEnumerator<ComNode> GetEnumerator() { return NodeList.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return NodeList.GetEnumerator(); }

        public ComGroup(string name)
        {
            Name = name;
            NodeList = new List<ComNode>();
            IpNodeMap = new Dictionary<string, ComNode>();
        }

        public void Add(ComNode node) {
            NodeList.Add(node);
            IpNodeMap.Add(node.IP, node);
        }

        public void Remove(ComNode node) {
            NodeList.Remove(node);
            IpNodeMap.Remove(node.IP);
        }

        public bool Contains(ComNode node)
        {
            return NodeList.Contains(node);
        }

        public ComNode GetNode(int index)
        {
            return NodeList[index];
        }

        public ComNode GetNodeByIp(string ip)
        {
            if (IpNodeMap.ContainsKey(ip)) return IpNodeMap[ip];
            else return null;
        }

        public int Count { get { return NodeList.Count; } }
    }
}
