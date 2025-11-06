import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReportService } from '../../services/report.service';
import { ChartConfiguration } from 'chart.js';

@Component({
  selector: 'app-report-generate',
  standalone: false,
  templateUrl: './report-generate.html',
//  styleUrls: ['./report-generate.css']
})
export class ReportGenerate {
  reportForm: FormGroup;
  isSubmitting = false;
  responseMessage = '';
  reportSummary: any = null;
  chartData: ChartConfiguration<any> | null = null;

  constructor(private fb: FormBuilder, private reportService: ReportService) {
    this.reportForm = this.fb.group({
      reportType: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      doctorId: [null]
    });
  }

  submit() {
    if (this.reportForm.invalid) return;

    this.isSubmitting = true;
    this.responseMessage = '';
    const payload = this.reportForm.value;

    this.reportService.generateReport(payload).subscribe({
      next: (res) => {
        this.responseMessage = '✅ Report generated';
        this.reportSummary = res;
        this.updateChart(); // ✅ Generate chart after report
        this.isSubmitting = false;
      },
      error: (err) => {
        console.error('Full error:', err);
        this.responseMessage = `failed to generate report: ${err.message || err.statusText || 'Unknown error'}`;
        this.isSubmitting = false;
      }
    });
  }

  updateChart() {
    if (!this.reportSummary?.metrics?.length) return;

    const type = this.reportSummary.type.toLowerCase();
    const metrics = this.reportSummary.metrics;
const labels = metrics.map((m: { label: string }) => m.label);
const values = metrics.map((m: { value: number }) => m.value);
const units = metrics.map((m: { unit: string }) => m.unit);


let chartType: 'bar' | 'line' | 'pie' | 'doughnut' | 'polarArea' | 'radar' | 'bubble' | 'Mixed Chart' | 'scatter' = 'bar';
    if (type === 'revenue') chartType = 'bar';
    if (type === 'utilization') chartType = 'pie';
    if (type === 'patientflow') chartType = 'line';

    const colors = ['#4c72b0', '#55a868', '#c44e52', '#8172b3', '#ccb974', '#64b5cd'];

    this.chartData = {
      type: chartType,
      data: {
        labels,
        datasets: [{
          label: `${this.reportSummary.type} Metrics`,
          data: values,
          backgroundColor: colors.slice(0, values.length),
          borderColor: '#333',
          fill: chartType !== 'line'
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: chartType !== 'bar' },
          tooltip: {
            callbacks: {
             label: (ctx: any) => {
  const unit = units[ctx.dataIndex];
  return `${ctx.raw} ${unit}`;
}

            }
          }
        },
        scales: chartType === 'bar' || chartType === 'line' ? {
          y: {
            beginAtZero: true,
            ticks: { precision: 0 }
          }
        } : undefined
      }
    };
  }
}

