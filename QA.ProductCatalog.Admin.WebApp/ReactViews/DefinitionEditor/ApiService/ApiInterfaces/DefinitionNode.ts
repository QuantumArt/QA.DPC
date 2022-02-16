import { IconName } from "@blueprintjs/core/lib/esm/components/icon/icon";

export interface IDefinitionNode {
  MissingInQp: boolean;
  parentId: string;
  IsDictionaries: boolean;
  encoded: boolean;
  expanded: boolean;
  checked: boolean;
  selected: boolean;
  Id: string;
  InDefinition: boolean;
  text: string;
  url: string | null;
  imageUrl: string | null;
  spriteCssClass: string | null;
  hasChildren: boolean;
  IconName: IconName;
}
