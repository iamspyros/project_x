/**
 * Quote Form - Line Item Management
 * Handles dynamic adding/removing of line items and real-time total calculation.
 */
(function () {
    'use strict';

    let lineItemIndex = document.querySelectorAll('.line-item-row').length;

    // Add line item button
    const addBtn = document.getElementById('addLineItem');
    if (addBtn) {
        addBtn.addEventListener('click', addLineItem);
    }

    // Remove line item (delegated)
    document.addEventListener('click', function (e) {
        const removeBtn = e.target.closest('.btn-remove-line');
        if (removeBtn) {
            const row = removeBtn.closest('.line-item-row');
            if (row) {
                row.remove();
                reindexLineItems();
                updateGrandTotal();
                toggleNoItemsMessage();
            }
        }
    });

    // Live calculation (delegated)
    document.addEventListener('input', function (e) {
        if (e.target.classList.contains('product-select') ||
            e.target.classList.contains('quantity-input') ||
            e.target.classList.contains('discount-input')) {
            const row = e.target.closest('.line-item-row');
            if (row) {
                updateLineTotal(row);
                updateGrandTotal();
            }
        }
    });

    // Initialize totals on page load
    document.querySelectorAll('.line-item-row').forEach(updateLineTotal);
    updateGrandTotal();

    function addLineItem() {
        const container = document.getElementById('lineItemsContainer');
        const idx = lineItemIndex++;

        const productOptions = getProductOptionsHtml();
        const html = `
            <div class="line-item-row row g-2 mb-2 align-items-end" data-index="${idx}">
                <div class="col-md-5">
                    <label class="form-label">Product</label>
                    <select name="Input.LineItems[${idx}].ProductId" class="form-select product-select" required>
                        <option value="">-- Select Product --</option>
                        ${productOptions}
                    </select>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Quantity</label>
                    <input name="Input.LineItems[${idx}].Quantity" type="number" min="1"
                           class="form-control quantity-input" value="1" required />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Discount %</label>
                    <input name="Input.LineItems[${idx}].DiscountPercent" type="number" min="0" max="100" step="0.01"
                           class="form-control discount-input" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Line Total</label>
                    <input type="text" class="form-control line-total" readonly tabindex="-1" />
                </div>
                <div class="col-md-1">
                    <button type="button" class="btn btn-outline-danger btn-remove-line" title="Remove">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>`;

        container.insertAdjacentHTML('beforeend', html);
        toggleNoItemsMessage();
    }

    function getProductOptionsHtml() {
        // Grab options from the first product-select (if exists) or from a hidden template
        const firstSelect = document.querySelector('.product-select');
        if (!firstSelect) return '';

        let html = '';
        firstSelect.querySelectorAll('option').forEach(opt => {
            if (opt.value) {
                html += `<option value="${opt.value}" data-price="${opt.dataset.price}" data-currency="${opt.dataset.currency}">${opt.textContent}</option>`;
            }
        });
        return html;
    }

    function updateLineTotal(row) {
        const select = row.querySelector('.product-select');
        const qtyInput = row.querySelector('.quantity-input');
        const discInput = row.querySelector('.discount-input');
        const totalInput = row.querySelector('.line-total');

        if (!select || !qtyInput || !totalInput) return;

        const selectedOption = select.options[select.selectedIndex];
        const price = parseFloat(selectedOption?.dataset?.price) || 0;
        const qty = parseInt(qtyInput.value) || 0;
        const discount = parseFloat(discInput?.value) || 0;

        const subtotal = price * qty;
        const discountAmount = subtotal * (discount / 100);
        const total = subtotal - discountAmount;

        totalInput.value = total.toFixed(2);
    }

    function updateGrandTotal() {
        let grand = 0;
        document.querySelectorAll('.line-total').forEach(input => {
            grand += parseFloat(input.value) || 0;
        });

        const grandEl = document.getElementById('grandTotal');
        if (grandEl) {
            grandEl.textContent = grand.toFixed(2);
        }
    }

    function reindexLineItems() {
        const rows = document.querySelectorAll('.line-item-row');
        rows.forEach((row, i) => {
            row.dataset.index = i;
            row.querySelectorAll('[name]').forEach(el => {
                el.name = el.name.replace(/\[\d+\]/, `[${i}]`);
            });
        });
        lineItemIndex = rows.length;
    }

    function toggleNoItemsMessage() {
        const msg = document.getElementById('noItemsMessage');
        const rows = document.querySelectorAll('.line-item-row');
        if (msg) {
            msg.style.display = rows.length === 0 ? 'block' : 'none';
        }
    }
})();
