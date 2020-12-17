import { Localization } from "Shared/Utils/Localization";

const strings = new Localization(window.task);

export const l = strings.get;
