import React from "react";
import { observer } from "mobx-react-lite";
import { Classes } from "@blueprintjs/core";
import cn from "classnames";
import { useStores } from "DefinitionEditor";
import { CustomTree } from "DefinitionEditor/Components";
import "./Style.scss";

const XmlTree = observer(() => {
  const { editorStore } = useStores();
  return (
    <div className="xml-tree-wrap">
      <CustomTree
        contents={editorStore.tree}
        onNodeExpand={editorStore.onNodeExpand}
        onNodeCollapse={editorStore.onNodeCollapse}
        className={cn(Classes.ELEVATION_0, "xml-tree")}
      />
    </div>
  );
});

export default XmlTree;
