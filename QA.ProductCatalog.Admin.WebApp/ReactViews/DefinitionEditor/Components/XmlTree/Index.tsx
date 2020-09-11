import React from "react";
import { observer } from "mobx-react-lite";
import { Classes, ITreeNode } from "@blueprintjs/core";
import cn from "classnames";
import { useStores } from "DefinitionEditor";
import { CustomTree } from "DefinitionEditor/Components";
import "./Style.scss";

const XmlTree = observer(() => {
  const { treeStore, controlsStore } = useStores();
  return (
    <div className="xml-tree-wrap">
      <CustomTree
        contents={treeStore.tree}
        onNodeExpand={treeStore.onNodeExpand}
        onNodeCollapse={treeStore.onNodeCollapse}
        onNodeClick={(node: ITreeNode, _nodePath: number[], e: React.MouseEvent<HTMLElement>) => {
          controlsStore.formMode &&
            controlsStore.submitFormSyntheticEvent &&
            controlsStore.submitFormSyntheticEvent(e);
          treeStore.onNodeClick(node, _nodePath, e);
        }}
        className={cn(Classes.ELEVATION_0, "xml-tree")}
      />
    </div>
  );
});

export default XmlTree;
