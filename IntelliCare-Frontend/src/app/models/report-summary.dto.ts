export interface ReportSummaryDto {
  reportID: number;
  type: string;
  generatedDate: string;
  metrics: any[];
  detailedData?: any;
}
 
 

// export interface MetricDto {
//   label: string;
//   value: number;
//   unit: string;
// }

// export interface ReportSummaryDto {
//   reportID: number;
//   reportType: string;
//   generatedOn: string;
//   metrics: MetricDto[];
// }