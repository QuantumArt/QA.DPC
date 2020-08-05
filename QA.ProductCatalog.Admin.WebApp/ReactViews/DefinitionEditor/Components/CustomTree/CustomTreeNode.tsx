import * as React from "react";
import { ITreeNodeProps, TreeNode } from "@blueprintjs/core";
import { observer } from "mobx-react";

@observer
export class CustomTreeNode<T = {}> extends React.Component<ITreeNodeProps<T>, {}> {
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
    return <this.typedTreeNode {...this.props} />;
  }
}
