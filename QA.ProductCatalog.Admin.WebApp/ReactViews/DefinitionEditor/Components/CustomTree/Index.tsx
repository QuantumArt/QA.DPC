import * as React from "react";
import classnames from "classnames";
import { observer } from "mobx-react";

import { CustomTreeNode } from "./CustomTreeNode";
import { ITreeNode, TreeNode, ITreeProps, TreeEventHandler } from "@blueprintjs/core";
import { isFunction } from "util";

const DISPLAYNAME_PREFIX = "Blueprint3";
class Classes {
  static NS = "bp3";
  public static TREE = `${Classes.NS}-tree`;
  public static TREE_NODE = `${Classes.NS}-tree-node`;
  public static TREE_ROOT = `${Classes.NS}-tree-root`;
  public static TREE_NODE_LIST = `${Classes.TREE_NODE}-list`;
}

interface Props<T> extends ITreeProps<T> {}
@observer
export class CustomTree<T = {}> extends React.Component<Props<T>, {}> {
  public static displayName = `${DISPLAYNAME_PREFIX}.Tree`;

  public static ofType<T>() {
    return CustomTree as new (props: Props<T>) => CustomTree<T>;
  }

  public static nodeFromPath(path: number[], treeNodes: ITreeNode[]): ITreeNode {
    if (path.length === 1) {
      return treeNodes[path[0]];
    }
    return CustomTree.nodeFromPath(path.slice(1), treeNodes[path[0]].childNodes);
  }

  private nodeRefs: { [nodeId: string]: HTMLElement } = {};

  componentWillUnmount(): void {}

  public render() {
    return (
      <div className={classnames(Classes.TREE, this.props.className)}>
        {this.renderNodes(this.props.contents, [], Classes.TREE_ROOT)}
      </div>
    );
  }

  public getNodeContentElement(nodeId: string | number): HTMLElement | undefined {
    return this.nodeRefs[nodeId];
  }

  private renderNodes(
    treeNodes: ITreeNode<T>[],
    currentPath?: number[],
    className?: string
  ): JSX.Element {
    if (treeNodes == null || treeNodes.length === 0) {
      return null;
    }

    const nodeItems = treeNodes.map((node, i) => {
      const elementPath = currentPath.concat(i);
      // tslint:disable-next-line:variable-name
      const TypedTreeNode = CustomTreeNode.ofType<T>();
      return [
        <TypedTreeNode
          {...node}
          key={node.id}
          contentRef={this.handleContentRef(+node.id)}
          depth={elementPath.length - 1}
          onClick={this.handleNodeClick}
          onContextMenu={this.handleNodeContextMenu}
          onCollapse={this.handleNodeCollapse}
          onDoubleClick={this.handleNodeDoubleClick}
          onExpand={this.handleNodeExpand}
          onMouseEnter={this.handleNodeMouseEnter}
          onMouseLeave={this.handleNodeMouseLeave}
          path={elementPath}
        />,
        node.isExpanded ? (
          <React.Fragment key={`${node.id}-r`}>
            {this.renderNodes(node.childNodes, elementPath)}
          </React.Fragment>
        ) : null
      ];
    });

    return <ul className={classnames(Classes.TREE_NODE_LIST, className)}>{nodeItems}</ul>;
  }

  private handleNodeCollapse = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeCollapse, node, e);
  };

  private handleNodeClick = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeClick, node, e);
  };

  private handleContentRef = (id: number) => (node: TreeNode<T>, element: HTMLElement | null) => {
    if (element != null) {
      this.nodeRefs[node.props.id] = element;
    } else {
      // don't want our object to get bloated with old keys
      delete this.nodeRefs[node.props.id];
    }
  };

  private handleNodeContextMenu = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeContextMenu, node, e);
  };

  private handleNodeDoubleClick = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeDoubleClick, node, e);
  };

  private handleNodeExpand = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeExpand, node, e);
  };

  private handleNodeMouseEnter = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeMouseEnter, node, e);
  };

  private handleNodeMouseLeave = (node: TreeNode<T>, e: React.MouseEvent<HTMLElement>) => {
    this.handlerHelper(this.props.onNodeMouseLeave, node, e);
  };

  private handlerHelper(
    handlerFromProps: TreeEventHandler,
    node: TreeNode<T>,
    e: React.MouseEvent<HTMLElement>
  ) {
    if (isFunction(handlerFromProps)) {
      const nodeData = CustomTree.nodeFromPath(node.props.path, this.props.contents);
      handlerFromProps(nodeData, node.props.path, e);
    }
  }
}
