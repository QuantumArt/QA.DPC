import React, { useEffect, useState, Dispatch, SetStateAction } from "react";
import { ItemPredicate, MultiSelect } from "@blueprintjs/select";
import { MenuItem } from "@blueprintjs/core";
import { ICronsTagModel, MONTH_UNITS, partToString, UNITS, WEEK_UNITS } from "Shared/Utils";
import _ from "lodash";
import "./Style.scss";
import { CronUnitType } from "Shared/Enums";

export interface ICronsMultiSelectProps {
  type: CronUnitType;
  setParsedCronsModel: Dispatch<SetStateAction<ICronsTagModel[]>>;
  parsedCronsModel: ICronsTagModel[] | undefined;
  isShouldClear: boolean;
}
interface ISelectItem {
  label: string;
  value: number;
}

export const CronsMultiselect = ({
  type,
  setParsedCronsModel,
  parsedCronsModel,
  isShouldClear
}: ICronsMultiSelectProps) => {
  const UNIT = UNITS.get(type);

  const items = React.useMemo((): ISelectItem[] => {
    switch (type) {
      case CronUnitType.WeekDays:
        return WEEK_UNITS.map((weekDay: string, index) => {
          return {
            label: weekDay,
            value: index
          };
        });
      case CronUnitType.Hours:
        return _.range(UNIT.min, UNIT.total).map(val => {
          return {
            label: String(val),
            value: val
          };
        });
      case CronUnitType.Minutes:
        return _.range(UNIT.min, UNIT.total).map(val => {
          return {
            label: String(val),
            value: val
          };
        });
      case CronUnitType.MonthDays:
        return _.range(UNIT.min, UNIT.total).map(val => {
          return {
            label: String(val),
            value: val
          };
        });
      case CronUnitType.Months:
        return MONTH_UNITS.map((month: string, index) => {
          return {
            label: month,
            value: index + 1
          };
        });
      default:
        return [];
    }
  }, []);

  const getFromParenState = () => {
    if (!parsedCronsModel) return [];
    const deep = _.flatten(parsedCronsModel.map(val => val.values));
    return deep.length ? items.filter(item => deep.includes(item.value)) : [];
  };

  const [multiSelectValues, setMultiSelectValues] = useState<ISelectItem[] | undefined>(
    getFromParenState()
  );

  const getSelectedItemIndex = (item: ISelectItem): number => {
    let foundIndex = -1;
    multiSelectValues.find((x: ISelectItem, index) => {
      const isFound = x.value === item.value;
      if (isFound) foundIndex = index;
      return isFound;
    });
    return foundIndex;
  };

  const isItemSelected = (item: ISelectItem) => {
    return getSelectedItemIndex(item) !== -1;
  };

  const deselectItem = (index: number) => {
    const newMultiSelectModel = multiSelectValues.filter((_film, i) => i !== index);
    setMultiSelectValues(newMultiSelectModel);
    setParsedCronsModel(partToString(newMultiSelectModel.map(x => x.value).sort(), UNIT, true));
  };

  const onRemoveTag = (_tag: string) => {
    const currentItem = parsedCronsModel.find(_x => _tag === _x.label);
    const newMultiSelectModel = multiSelectValues.filter(
      x => !currentItem.values.includes(x.value)
    );
    setMultiSelectValues(newMultiSelectModel);
    setParsedCronsModel(partToString(newMultiSelectModel.map(x => x.value).sort(), UNIT, true));
  };

  const onItemSelect = (x: ISelectItem) => {
    if (!isItemSelected(x)) {
      const newMultiSelectModel = [...multiSelectValues, x];
      setMultiSelectValues(newMultiSelectModel);
      setParsedCronsModel(partToString(newMultiSelectModel.map(x => x.value).sort(), UNIT, true));
    } else {
      deselectItem(getSelectedItemIndex(x));
    }
  };

  const filterItems: ItemPredicate<ISelectItem> = (query, item, _index, exactMatch) => {
    const normalizedTitle = item.label.toLowerCase();
    const normalizedQuery = query.toLowerCase();

    if (exactMatch) {
      return normalizedTitle === normalizedQuery;
    } else {
      return `${item.label}. ${normalizedTitle}`.indexOf(normalizedQuery) >= 0;
    }
  };

  useEffect(() => {
    if (isShouldClear) {
      setMultiSelectValues([]);
      setParsedCronsModel([]);
    }
  }, [isShouldClear]);

  return (
    <MultiSelect<ISelectItem | ICronsTagModel>
      placeholder="Поиск..."
      itemPredicate={filterItems}
      itemRenderer={(x: ISelectItem, { modifiers, handleClick }) => {
        if (!modifiers.matchesPredicate) {
          return null;
        }
        return (
          <MenuItem
            className="crons-menuitem"
            active={modifiers.active}
            icon={isItemSelected(x) ? "tick" : "blank"}
            key={x.value}
            onClick={handleClick}
            text={x.label}
            shouldDismissPopover={false}
          />
        );
      }}
      scrollToActiveItem={false}
      noResults={<MenuItem disabled={true} text="No results." />}
      items={items}
      popoverProps={{
        popoverClassName: "crons-select-popover"
      }}
      onItemSelect={onItemSelect}
      tagInputProps={{
        onRemove: onRemoveTag
      }}
      tagRenderer={(x: ICronsTagModel) => {
        return x.label;
      }}
      selectedItems={parsedCronsModel}
    />
  );
};
