export interface IDefinitionNode {
  MissingInQp: boolean;
  IsDictionaries: boolean;
  encoded: boolean;
  expanded: boolean;
  checked: boolean;
  selected: boolean;
  Id: string;
  NotInDefinition: boolean;
  text: string;
  url: string | null;
  imageUrl: string | null;
  spriteCssClass: string | null;
  hasChildren: boolean;
}
