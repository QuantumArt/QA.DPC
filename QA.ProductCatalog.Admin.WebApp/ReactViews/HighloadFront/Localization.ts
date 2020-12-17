import { Localization } from "Shared/Utils/Localization";

const inst = new Localization(window.highloadFront);
export const l = inst.get;
