import * as React from "react";
import { ITreeNodeProps, TreeNode } from "@blueprintjs/core";
import cn from "classnames";
import { observer } from "mobx-react";
import { storesCtx } from "DefinitionEditor/Stores";
import { OperationState } from "Shared/Enums";
import "./CustomTreeNode.scss";

@observer
export class CustomTreeNode<T = {}> extends React.Component<ITreeNodeProps<T>, {}> {
  public static contextType = storesCtx;

  public static ofType<T>() {
    return CustomTreeNode as new (props: ITreeNodeProps<T>) => CustomTreeNode<T>;
  }

  typedTreeNode = TreeNode.ofType<T>();

  shouldComponentUpdate(nextProps: ITreeNodeProps<T>): boolean {
    return !(
      this.props.isExpanded === nextProps.isExpanded &&
      this.props.isSelected === nextProps.isSelected
    );
  }

  render() {
    return this.context.editorStore.operationState === OperationState.Pending ? (
      <div className={cn("bp3-skeleton", `xml-tree-node-content-${this.props.depth}`)}>
        <p>Loading</p>
      </div>
    ) : (
      <this.typedTreeNode {...this.props} />
    );
  }
}
