﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZDevTools.Collections
{
    /// <summary>
    /// 可用于查找与操作的节点结构
    /// </summary>
    /// <typeparam name="TTreeNode">节点泛型参数</typeparam>
    /// <typeparam name="TKey">节点Id泛型参数</typeparam>
    public class TreeNode<TTreeNode, TKey> : TreeNode<TTreeNode>
        where TTreeNode : TreeNode<TTreeNode, TKey>
    {
        TKey _id;
        /// <summary>
        /// 节点 Id
        /// </summary>
        public TKey Id
        {
            get { return _id; }
            set
            {
                if (Tree != null || Children.Count > 0)
                    throw new TreeNodeException<TTreeNode, TKey>("不能更改附加在树中的或者有子节点的节点的Id！", (TTreeNode)this);
                _id = value;
            }
        }

        TKey _parentId;
        /// <summary>
        /// 父节点 Id
        /// </summary>
        public TKey ParentId
        {
            get { return _parentId; }
            set
            {
                if (Tree != null || Parent != null)
                    throw new TreeNodeException<TTreeNode, TKey>("不能更改已附加在树中的或者具有父节点引用的节点的父节点Id！", (TTreeNode)this);
                _parentId = value;
            }
        }

        /// <summary>
        /// 该节点所属树引用
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public Tree<TTreeNode, TKey> Tree { get; internal set; }

        #region 解析
        /// <summary>
        /// 该方法用于解析节点列表，解析后组织为节点树结构，并获得唯一根节点（忽略返回的平化节点字典）
        /// </summary>
        /// <param name="nodes">treeNode可枚举对象</param>
        /// <remarks>您必须保证nodes中有且仅有一个根节点，否则会报错。节点排序：后入先出，因此，nodes中排序靠后的节点会最先出现在节点树中</remarks>
        public static TTreeNode Parse(IEnumerable<TTreeNode> nodes) => Parse(null, nodes, out _);

        /// <summary>
        /// 该方法用于解析节点列表，解析后组织为节点树结构，并获得唯一根节点
        /// </summary>
        /// <param name="nodes">treeNode可枚举对象</param>
        /// <param name="flattenNodes">平化的节点字典</param>
        /// <param name="tree">所属Tree</param>
        /// <remarks>您必须保证nodes中有且仅有一个根节点，否则会报错。节点排序：后入先出，因此，nodes中排序靠后的节点会最先出现在节点树中。
        /// 如果整理出错，请不要再次使用这些节点，因为节点状态已更改并且无法保证处于未附加状态中。
        /// </remarks>
        public static TTreeNode Parse(Tree<TTreeNode, TKey> tree, IEnumerable<TTreeNode> nodes, out Dictionary<TKey, TTreeNode> flattenNodes)
        {
            var treeNodes = nodes.ToList();

            flattenNodes = treeNodes.ToDictionary(node => node.Id);

            //整理为树
            for (int i = treeNodes.Count - 1; i > -1; i--)
            {
                var current = treeNodes[i];
                //检查条件，不允许附加到其他树的节点附加到本树
                if (current.Tree != null)
                    throw new TreeNodeException<TTreeNode, TKey>("同一个节点不能同时被多个树引用！", current);
                current.Tree = tree;
                TTreeNode parent;
                flattenNodes.TryGetValue(current.ParentId, out parent);
                if (!current.ParentId.Equals(current.Id)) //有父节点，否则父节点是本身也就是根节点，根节点的Parent属性值是null
                {
                    current.Parent = parent;
                    ((IList<TTreeNode>)parent.Children).Add(current);
                    treeNodes.Remove(current);
                }
            }

            if (treeNodes.Count > 1)
                throw new TreeNodeException<TTreeNode, TKey>("整理失败，发现多个节点疑似根节点！");

            return treeNodes[0];
        }
        #endregion

        #region 判断
        /// <summary>
        /// 当前节点及子节点中是否包含具有指定Id的节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(TKey id)
        {
            return contains((TTreeNode)this, id);
        }
        static bool contains(TTreeNode node, TKey id)
        {
            if (node.Id.Equals(id))
                return true;
            else
                foreach (var child in node.Children)
                    if (contains(child, id))
                        return true;
            return false;
        }

        #endregion

        #region 查找
        /// <summary>
        /// 在当前节点及子节点中查找具有指定Id的节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TTreeNode Find(TKey id)
        {
            return find((TTreeNode)this, id);
        }
        static TTreeNode find(TTreeNode node, TKey id)
        {
            if (node.Id.Equals(id))
                return node;
            else
                foreach (var child in node.Children)
                {
                    var result = find(child, id);
                    if (result != null)
                        return result;
                }
            return null;
        }
        #endregion
    }
}
