import { Component, OnInit, inject } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { readApiError } from '../../shared/api-error';
import { OrderApiService } from '../order-api.service';
import { MenuItem, formatMoney } from '../order.model';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.css'
})
export class OrderFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(OrderApiService);
  private readonly router = inject(Router);

  menu: MenuItem[] = [];
  menuLoading = true;
  saving = false;
  error = '';
  readonly formatMoney = formatMoney;

  readonly form = this.fb.nonNullable.group({
    clientReference: ['', Validators.maxLength(80)],
    notes: ['', Validators.maxLength(1000)],
    lineItems: this.fb.array([this.lineGroup()])
  });

  get lines(): FormArray {
    return this.form.controls.lineItems;
  }

  ngOnInit(): void {
    this.api.listMenu().subscribe({
      next: (menu) => {
        this.menu = menu;
        this.menuLoading = false;
      },
      error: (err) => {
        this.error = readApiError(err);
        this.menuLoading = false;
      }
    });
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

  priceFor(menuItemId: string): number {
    if (!menuItemId) {
      return 0;
    }
    return this.menu.find((m) => m.id === menuItemId)?.unitPrice ?? 0;
  }

  dishLabel(item: MenuItem): string {
    return `${item.name} (${formatMoney(item.unitPrice)})`;
  }

  submit(): void {
    this.error = '';
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.error = 'Choose at least one dish and fill in the required fields before submitting.';
      return;
    }

    const value = this.form.getRawValue();
    if (value.lineItems.some((line) => !line.menuItemId)) {
      this.error = 'Please select a dish for every line.';
      return;
    }

    this.saving = true;
    this.api.submit({
      clientReference: value.clientReference?.trim() ? value.clientReference.trim() : undefined,
      notes: value.notes?.trim() ? value.notes : undefined,
      lineItems: value.lineItems.map((line) => ({
        menuItemId: line.menuItemId,
        quantity: Number(line.quantity)
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
      menuItemId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
  }
}
