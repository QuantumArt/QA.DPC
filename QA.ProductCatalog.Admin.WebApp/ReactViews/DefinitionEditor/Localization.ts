import { Localization } from "Shared/Utils/Localization";

const inst = new Localization(window.definitionEditor);
export const l = inst.get;
