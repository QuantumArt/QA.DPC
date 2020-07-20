/**
 * Global definitions for developement
 * DO NOT IMPORT ANYTHING OR YOU WILL BREAK BUILD PROCESS!
 */

declare interface Window {
  QP: {
    //for Task view
    Tasks: {
      tableFields: {
        userName: string;
        status: number;
        statusValues: { text: string; value: number }[];
        schedule: boolean;
        progress: number;
        name: string;
        created: string;
        lastStatusChange: string;
        message: string;
      };
      tableFilters: {
        //отображение названий в селекте фильтров
        messages: {
          isFalse: string;
          isTrue: string;
          filter: string;
          clear: string;
        };
      };
    };
  };
}
