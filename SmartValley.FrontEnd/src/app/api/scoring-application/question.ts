export interface Question {
  id: number;
  key: string;
  type: number;
  extendedInfo: string;
  parentId: number;
  parentTriggerValue: string;
  answer: string;
  order: number;
}
