﻿import React, { useCallback, useLayoutEffect } from "react";
import { observer, useLocalStore } from "mobx-react-lite";
import SplitPane from "react-split-pane";
import debounce from "lodash/debounce";
import { ToolBar, XmlEditor, XmlTree } from "DefinitionEditor/Components";
import "./Root.scss";
import UnsavedChangesDialog from "DefinitionEditor/Components/UnsavedChangesDialog";

const Root = observer(() => {
  const topOffset = 60;
  const defaultSplitSize = 400;
  const state = useLocalStore(() => ({
    height: `${window.innerHeight - topOffset}px`,
    width: `${window.innerWidth - defaultSplitSize}`,
    splitWidth: 0,
    setHeight(height: string) {
      this.height = height;
    },
    setWidth(width: string) {
      this.width = width;
    },
    setSplitWidth(width: number) {
      this.splitWidth = width;
    }
  }));
  useLayoutEffect(() => {
    const onResize = debounce(() => {
      state.setHeight(`${window.innerHeight - topOffset}px`);
      state.setWidth(
        `${window.innerWidth -
          (state.splitWidth === 0 ? defaultSplitSize : state.splitWidth) -
          1}px`
      );
    }, 100);
    window.addEventListener("resize", onResize);
    onResize();
    return () => {
      window.removeEventListener("resize", onResize);
    };
  }, []);
  const onSplitPaneChange = useCallback(
    debounce((size: number) => {
      state.setSplitWidth(size);
      state.setWidth(`${window.innerWidth - size - 1}px`);
      state.setHeight(`${window.innerHeight - topOffset}px`);
    }, 100),
    [state.width]
  );
  return (
    <>
      <UnsavedChangesDialog />
      <ToolBar />
      <SplitPane
        split="vertical"
        minSize={250}
        maxSize={700}
        defaultSize={defaultSplitSize}
        style={{ height: state.height }}
        onChange={onSplitPaneChange}
      >
        <XmlTree />
        <XmlEditor height={state.height} width={state.width} />
      </SplitPane>
    </>
  );
});

export default Root;
