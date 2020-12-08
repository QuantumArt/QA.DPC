import { Localization } from "Shared/Utils/Localization";

const inst = new Localization(window.partialSend);
export const l = inst.get;
