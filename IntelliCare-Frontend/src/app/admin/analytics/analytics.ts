import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReportService } from '../../services/report.service';
import { ChartConfiguration } from 'chart.js';
import { ReportSummaryDto } from '../../models/report-summary.dto';
import { ReportDetailDto } from '../../models/report-detail.dto';
import { Doctor } from '../../models/doctor.model';
@Component({
  selector: 'app-analytics',
  standalone: false,
  templateUrl: './analytics.html',
  styleUrls: ['./analytics.css']
})
export class Analytics implements OnInit {
  activeTab: 'generate' | 'list' | 'detail' = 'generate';

  // Generate Report
  reportForm: FormGroup;
  isSubmitting = false;
  responseMessage = '';
  reportSummary: any = null;
  chartData: ChartConfiguration<any> | null = null;

  // Report List
  allReports: ReportSummaryDto[] = [];
  reports: ReportSummaryDto[] = [];
    // Data arrays
//  doctors: any[] = [];
  doctors: Doctor[] = [];
  isLoading = false;
  errorMessage = '';
  filterType: string = '';
  filterID: number | null = null;

  // Report Detail
  selectedReport: ReportDetailDto | null = null;
  parsedMetrics: any[] = [];
  parsedDetails: any = {};

  constructor(private fb: FormBuilder, private reportService: ReportService) {
    this.reportForm = this.fb.group({
      reportType: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      doctorId: [null]
    });
  }

  ngOnInit(): void {
    this.fetchReports();
    this.loadDoctors();
  }

  loadDoctors(): void {
  this.reportService.getAllDoctors().subscribe({
    next: (data) => {
      this.doctors = data;
    },
    error: (err) => {
      console.error('Failed to load doctors', err);
    }
  });
}

  /** ---------------- Generate Report ---------------- **/
  submit() {
    if (this.reportForm.invalid) return;
    this.isSubmitting = true;
    this.responseMessage = '';
    const payload = this.reportForm.value;

    this.reportService.generateReport(payload).subscribe({
      next: (res) => {
        this.responseMessage = '✅ Report generated successfully!';
        this.reportSummary = res;
        this.updateChart();
        this.isSubmitting = false;
      },
      error: (err) => {
        this.responseMessage = `❌ Failed to generate report: ${err.message || 'Unknown error'}`;
        this.isSubmitting = false;
      }
    });
  }

  updateChart() {
    if (!this.reportSummary?.metrics?.length) return;
    const type = this.reportSummary.type.toLowerCase();
    const metrics = this.reportSummary.metrics;
    const labels = metrics.map((m: any) => m.label);
    const values = metrics.map((m: any) => m.value);
    const units = metrics.map((m: any) => m.unit);

    let chartType: 'bar' | 'line' | 'pie' = 'bar';
    if (type === 'revenue') chartType = 'bar';
    if (type === 'utilization') chartType = 'pie';
    if (type === 'patientflow') chartType = 'line';

    this.chartData = {
      type: chartType,
      data: {
        labels,
        datasets: [{
          label: `${this.reportSummary.type} Metrics`,
          data: values,
          backgroundColor: ['#4c72b0', '#55a868', '#c44e52'],
          borderColor: '#333'
        }]
      },
      options: {
        responsive: true,
        plugins: {
          tooltip: {
            callbacks: {
              label: (ctx: any) => `${ctx.raw} ${units[ctx.dataIndex]}`
            }
          }
        }
      }
    };
  }

  /** ---------------- Report List ---------------- **/
  fetchReports(): void {
    this.isLoading = true;
    this.reportService.getAllReports().subscribe({
      next: (res: ReportSummaryDto[]) => {
        this.allReports = res.sort((a, b) => a.reportID - b.reportID);
        this.reports = [...this.allReports];
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load reports.';
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    this.reports = this.allReports.filter(report => {
      const matchesType = this.filterType ? report.type.toUpperCase() === this.filterType.toUpperCase() : true;
      const matchesID = this.filterID ? report.reportID === this.filterID : true;
      return matchesType && matchesID;
    });
  }

  /** ---------------- Report Detail ---------------- **/
  viewReport(reportID: number): void {
    this.activeTab = 'detail';
    this.isLoading = true;
    this.reportService.getReportDetail(reportID).subscribe({
      next: (res: ReportDetailDto) => {
        this.selectedReport = res;
        this.parsedMetrics = JSON.parse(res.metricsJson);
        try {
          this.parsedDetails = JSON.parse(res.detailedDataJson);
        } catch {
          this.parsedDetails = {};
        }
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load report detail.';
        this.isLoading = false;
      }
    });
  }

  deleteReport(reportID: number): void {
    if (!confirm('Are you sure you want to delete this report?')) return;
    this.reportService.deleteReport(reportID).subscribe({
      next: () => {
        this.allReports = this.allReports.filter(r => r.reportID !== reportID);
        this.applyFilters();
      }
    });
  }
}