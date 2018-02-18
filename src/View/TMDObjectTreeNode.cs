﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libLSD.Formats;
using LSDView.controller;
using LSDView.graphics;
using LSDView.view;

namespace LSDView.view
{
    public class TMDObjectTreeNode : RenderableMeshTreeNode, IDataTypeTreeNode
    {
        public TMD Tmd { get; }
        public int ObjectIndex { get; }

        public TMDObjectTreeNode(string text, IRenderable r, TMD tmd, int objectIndex) : base(text, r)
        {
            Tmd = tmd;
            ObjectIndex = objectIndex;
        }

        public void Accept(IOutlineTreeViewVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
