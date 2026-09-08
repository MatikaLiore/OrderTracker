import { Component, inject } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OrderApiService, readApiError } from './order-api.service';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.css'
})
export class OrderFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(OrderApiService);
  private readonly router = inject(Router);

  saving = false;
  error = '';

  readonly form = this.fb.nonNullable.group({
    externalReference: ['', [Validators.required, Validators.maxLength(80)]],
    customerName: ['', [Validators.required, Validators.maxLength(120)]],
    customerEmail: ['', [Validators.required, Validators.email]],
    currency: ['USD', Validators.required],
    notes: ['', Validators.maxLength(1000)],
    lineItems: this.fb.array([this.lineGroup()])
  });

  get lines(): FormArray {
    return this.form.controls.lineItems;
  }

  addLine(): void {
    this.lines.push(this.lineGroup());
  }

  removeLine(index: number): void {
    if (this.lines.length === 1) {
      return;
    }
    this.lines.removeAt(index);
  }

  submit(): void {
    this.error = '';
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.error = 'Fill in the required fields before submitting.';
      return;
    }

    const value = this.form.getRawValue();
    this.saving = true;
    this.api.submit({
      externalReference: value.externalReference,
      customer: { name: value.customerName, email: value.customerEmail },
      currency: value.currency,
      notes: value.notes?.trim() ? value.notes : undefined,
      lineItems: value.lineItems.map((line) => ({
        sku: line.sku,
        name: line.name,
        quantity: Number(line.quantity),
        unitPrice: Number(line.unitPrice)
      }))
    }).subscribe({
      next: (result) => {
        this.saving = false;
        void this.router.navigate(['/orders', result.order.id], {
          state: { replay: !result.created }
        });
      },
      error: (err) => {
        this.saving = false;
        this.error = readApiError(err);
      }
    });
  }

  private lineGroup() {
    return this.fb.nonNullable.group({
      sku: ['', Validators.required],
      name: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]]
    });
  }
}
