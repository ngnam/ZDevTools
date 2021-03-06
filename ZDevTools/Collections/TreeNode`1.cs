﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ZDevTools.Collections
{
    /// <summary>
    /// 最基本的节点结构
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    public class TreeNode<T>
        where T : TreeNode<T>
    {
        /// <summary>
        /// 孩子节点
        /// </summary>
        [NotMapped]
        public IReadOnlyList<T> Children { get; internal set; } = new List<T>();

        /// <summary>
        /// 父节点
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public T Parent { get; internal set; }

        /// <summary>
        /// 获取当前节点在父节点中的位置，-1代表没有父节点
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public int Index
        {
            get
            {
                if (Parent == null)
                    return -1;
                else
                    return ((List<T>)Parent.Children).IndexOf((T)this);
            }
        }

        #region 线性化
        /// <summary>
        /// 将所有子节点线性化为列表
        /// </summary>
        /// <returns></returns>
        public List<T> SubToList()
        {
            var list = new List<T>();

            linearSub((T)this, list);

            return list;
        }
        static void linearSub(T menuItem, List<T> list)
        {
            foreach (var item in menuItem.Children)
            {
                list.Add(item);
                linearSub(item, list);
            }
        }

        /// <summary>
        /// 将当前节点及子节点线性化为列表
        /// </summary>
        /// <returns></returns>
        public List<T> AllToList()
        {
            var list = new List<T>();
            linear((T)this, list);
            return list;
        }
        static void linear(T menuItem, List<T> list)
        {
            list.Add(menuItem);

            foreach (var item in menuItem.Children)
            {
                linear(item, list);
            }
        }
        #endregion

        #region 查找
        /// <summary>
        /// 在当前节点及其所有子节点中查找所有符合断言的节点
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<T> FindAll(Func<T, bool> predicate)
        {
            var list = new List<T>();
            linear((T)this, list, predicate);
            return list;
        }
        static void linear(T menuItem, List<T> list, Func<T, bool> predicate)
        {
            if (predicate(menuItem))
                list.Add(menuItem);

            foreach (var item in menuItem.Children)
            {
                linear(item, list, predicate);
            }
        }

        /// <summary>
        /// 查找第一个发现的可以通过断言的节点
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public T Find(Func<T, bool> predicate)
        {
            return find((T)this, predicate);
        }
        static T find(T node, Func<T, bool> predicate)
        {
            if (predicate(node))
                return node;
            else
                foreach (var child in node.Children)
                {
                    var result = find(child, predicate);
                    if (result != null)
                        return result;
                }
            return null;
        }
        #endregion

        #region 判断
        /// <summary>
        /// 当前节点及其子节点是否包含能够通过断言的节点
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool Contains(Func<T, bool> predicate)
        {
            return contains((T)this, predicate);
        }
        static bool contains(T node, Func<T, bool> predicate)
        {
            if (predicate(node))
                return true;
            else
                foreach (var child in node.Children)
                    if (contains(child, predicate))
                        return true;
            return false;
        }
        #endregion
    }
}
