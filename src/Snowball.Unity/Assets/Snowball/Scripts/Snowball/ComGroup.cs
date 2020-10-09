using System;
using System.Collections.Generic;
using System.Net;

namespace Snowball
{
    public class ComGroup : IEnumerable<ComNode>
    {
        public string Name { get; private set; }

        List<ComNode> NodeList { get; set; }
        Dictionary<IPEndPoint, ComNode> EndPointNodeMap { get; set; }

        public IEnumerator<ComNode> GetEnumerator() { return NodeList.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return NodeList.GetEnumerator(); }

        public ComGroup(string name)
        {
            Name = name;
            NodeList = new List<ComNode>();
            EndPointNodeMap = new Dictionary<IPEndPoint, ComNode>();
        }

        public void Add(ComNode node) {
            NodeList.Add(node);
            EndPointNodeMap.Add(node.TcpEndPoint, node);
        }

        public void Remove(ComNode node) {
            NodeList.Remove(node);
            EndPointNodeMap.Remove(node.TcpEndPoint);
        }

        public bool Contains(ComNode node)
        {
            return NodeList.Contains(node);
        }

        public ComNode GetNode(int index)
        {
            return NodeList[index];
        }

        public ComNode GetNodeByEndPoint(IPEndPoint endPoint)
        {
            ComNode node;
            if (EndPointNodeMap.TryGetValue(endPoint, out node)) return node;
            else return null;
        }

        public int Count { get { return NodeList.Count; } }
    }
}
