import React from "react";
import { observer } from "mobx-react-lite";
import { Classes, Tree } from "@blueprintjs/core";
import cn from "classnames";
import { CustomTree } from "DefinitionEditor/Components/CustomTree";
import { useStores } from "DefinitionEditor";
import "./Style.scss";
import { toJS } from "mobx";

interface Props {}

const XmlTree = observer<Props>(() => {
  const { editorStore } = useStores();
  console.log(editorStore.tree);
  return (
    <CustomTree
      contents={editorStore.tree}
      onNodeExpand={editorStore.onNodeExpand}
      onNodeCollapse={editorStore.onNodeCollapse}
      className={cn(Classes.ELEVATION_0, "xml-tree")}
    />
  );
});

export default XmlTree;
