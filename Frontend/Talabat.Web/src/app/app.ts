import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, of, throwError } from 'rxjs';
import {
  loadStripe,
  Stripe,
  StripeElements,
  StripePaymentElement,
} from '@stripe/stripe-js';
import {
  Basket,
  BasketItem,
  DeliveryMethod,
  LookupItem,
  OrdersSummary,
  Product,
  ProductPage,
  ShippingAddress,
  UserOrder,
  UserProfile,
} from './models';
import { StoreApiService } from './store-api.service';

type StoreView = 'shop' | 'orders' | 'cache';
type AuthMode = 'login' | 'register';

@Component({
  selector: 'app-root',
  imports: [DatePipe, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  view: StoreView = 'shop';
  products: Product[] = [];
  brands: LookupItem[] = [];
  types: LookupItem[] = [];
  page: ProductPage | null = null;
  pageIndex = 1;
  searchText = '';
  search = '';
  selectedBrand = '';
  selectedType = '';
  sort = '';
  loadingProducts = true;
  productError = '';

  basket: Basket = { id: this.getBasketId(), items: [] };
  basketOpen = false;
  basketBusy = false;
  checkoutOpen = false;
  checkoutBusy = false;
  checkoutError = '';
  paymentReady = false;
  selectedDeliveryId = 0;
  deliveryMethods: DeliveryMethod[] = [];
  shippingAddress: ShippingAddress = this.emptyAddress();
  private stripe: Stripe | null = null;
  private stripeElements: StripeElements | null = null;
  private paymentElement: StripePaymentElement | null = null;

  authOpen = false;
  authMode: AuthMode = 'login';
  authBusy = false;
  authError = '';
  authEmail = '';
  authPassword = '';
  registerName = '';
  registerUsername = '';
  registerPhone = '';
  user: UserProfile | null = null;

  orders: OrdersSummary | null = null;
  ordersLoading = false;
  ordersError = '';
  expandedOrderId: number | null = null;
  cacheKeys: string[] = [];
  cacheLoading = false;
  cacheError = '';

  selectedProduct: Product | null = null;
  toastMessage = '';
  toastKind: 'success' | 'error' = 'success';
  private toastTimer: ReturnType<typeof setTimeout> | undefined;

  constructor(private readonly api: StoreApiService) {}

  ngOnInit(): void {
    this.loadCatalogLookups();
    this.loadProducts();
    this.loadBasket();

    if (localStorage.getItem('talabat.access-token')) {
      this.api.getCurrentUser().subscribe({
        next: (user) => {
          this.user = user;
          this.completeRedirectedPayment();
        },
        error: () => this.logout(false),
      });
    } else {
      this.completeRedirectedPayment();
    }
  }

  get basketCount(): number {
    return this.basket.items.reduce((count, item) => count + item.quantity, 0);
  }

  get subtotal(): number {
    return this.basket.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  get selectedDelivery(): DeliveryMethod | undefined {
    return this.deliveryMethods.find((method) => method.id === this.selectedDeliveryId);
  }

  get checkoutTotal(): number {
    return this.subtotal + (this.selectedDelivery?.cost ?? 0);
  }

  get pageCount(): number {
    return this.page ? Math.max(1, Math.ceil(this.page.count / this.page.pageSize)) : 1;
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.pageCount }, (_, index) => index + 1);
  }

  scrollToCatalog(): void {
    document.getElementById('catalog')?.scrollIntoView({ behavior: 'smooth' });
  }

  loadCatalogLookups(): void {
    this.api.getBrands().subscribe({
      next: (brands) => (this.brands = brands),
      error: (error: unknown) => this.showToast(this.errorMessage(error), 'error'),
    });
    this.api.getTypes().subscribe({
      next: (types) => (this.types = types),
      error: (error: unknown) => this.showToast(this.errorMessage(error), 'error'),
    });
  }

  loadProducts(): void {
    this.loadingProducts = true;
    this.productError = '';
    this.api
      .getProducts({
        pageIndex: this.pageIndex,
        search: this.search,
        brandId: this.selectedBrand ? Number(this.selectedBrand) : undefined,
        typeId: this.selectedType ? Number(this.selectedType) : undefined,
        sort: this.sort,
      })
      .subscribe({
        next: (page) => {
          this.page = page;
          this.products = page.data;
          this.loadingProducts = false;
        },
        error: (error: unknown) => {
          this.productError = this.errorMessage(error);
          this.loadingProducts = false;
        },
      });
  }

  submitSearch(): void {
    this.search = this.searchText.trim();
    this.pageIndex = 1;
    this.loadProducts();
  }

  updateFilters(): void {
    this.pageIndex = 1;
    this.loadProducts();
  }

  clearFilters(): void {
    this.searchText = '';
    this.search = '';
    this.selectedBrand = '';
    this.selectedType = '';
    this.sort = '';
    this.pageIndex = 1;
    this.loadProducts();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.pageCount || page === this.pageIndex) return;
    this.pageIndex = page;
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  addToBasket(product: Product): void {
    const items = [...this.basket.items];
    const existing = items.find((item) => item.id === product.id);
    if (existing) {
      existing.quantity += 1;
    } else {
      items.push(this.toBasketItem(product));
    }
    this.saveBasket({ ...this.basket, items }, 'Added to your bag');
  }

  changeQuantity(item: BasketItem, quantity: number): void {
    const items =
      quantity < 1
        ? this.basket.items.filter((candidate) => candidate.id !== item.id)
        : this.basket.items.map((candidate) =>
            candidate.id === item.id ? { ...candidate, quantity } : candidate,
          );
    if (items.length === 0) {
      this.removeBasket();
      return;
    }
    this.saveBasket({ ...this.basket, items });
  }

  removeBasket(): void {
    this.basketBusy = true;
    this.api.deleteBasket(this.basket.id).subscribe({
      next: () => {
        this.basket = { id: this.basket.id, items: [] };
        this.basketBusy = false;
        this.checkoutOpen = false;
        this.paymentReady = false;
        this.clearStripeElement();
        this.showToast('Your bag is empty');
      },
      error: (error: unknown) => {
        this.basketBusy = false;
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.basket = { id: this.basket.id, items: [] };
          return;
        }
        this.showToast(this.errorMessage(error), 'error');
      },
    });
  }

  openCheckout(): void {
    if (!this.basket.items.length) return;
    if (!this.user) {
      this.basketOpen = false;
      this.openAuth('login');
      this.showToast('Sign in to continue to checkout', 'error');
      return;
    }

    this.checkoutError = '';
    this.checkoutOpen = true;
    this.api.getDeliveryMethods().subscribe({
      next: (methods) => {
        this.deliveryMethods = methods;
        this.selectedDeliveryId =
          this.basket.deliveryMethodId ?? methods[0]?.id ?? 0;
      },
      error: (error: unknown) => {
        this.checkoutError = this.errorMessage(error);
      },
    });
    this.api.getUserAddress().subscribe({
      next: (address) => {
        if (address && this.isAddressEmpty()) this.shippingAddress = { ...address };
      },
      error: (error: unknown) => {
        this.checkoutError = this.errorMessage(error);
      },
    });
  }

  async continueToPayment(): Promise<void> {
    if (!this.selectedDeliveryId || !this.isAddressValid()) {
      this.checkoutError = 'Choose a delivery option and complete every address field.';
      return;
    }
    this.checkoutBusy = true;
    this.checkoutError = '';
    const checkoutState = {
      basketId: this.basket.id,
      deliveryMethodId: this.selectedDeliveryId,
      address: this.shippingAddress,
    };
    sessionStorage.setItem('talabat.pending-checkout', JSON.stringify(checkoutState));

    this.api
      .saveBasket({ ...this.basket, deliveryMethodId: this.selectedDeliveryId })
      .subscribe({
        next: (basket) => {
          this.basket = basket;
          this.api.createPaymentIntent(this.basket.id).subscribe({
            next: (paymentBasket) => {
              this.basket = paymentBasket;
              this.mountPayment(paymentBasket.clientSecret);
            },
            error: (error: unknown) => {
              this.checkoutError = this.errorMessage(error);
              this.checkoutBusy = false;
            },
          });
        },
        error: (error: unknown) => {
          this.checkoutError = this.errorMessage(error);
          this.checkoutBusy = false;
        },
      });
  }

  async confirmPayment(): Promise<void> {
    if (!this.stripe || !this.stripeElements || this.checkoutBusy) return;
    this.checkoutBusy = true;
    this.checkoutError = '';
    const result = await this.stripe.confirmPayment({
      elements: this.stripeElements,
      confirmParams: { return_url: window.location.href },
      redirect: 'if_required',
    });
    this.checkoutBusy = false;

    if (result.error) {
      this.checkoutError = result.error.message ?? 'Payment could not be confirmed.';
      return;
    }
    if (result.paymentIntent?.status === 'succeeded') {
      this.createOrderAfterPayment();
      return;
    }
    this.checkoutError = `Payment status: ${result.paymentIntent?.status ?? 'unknown'}.`;
  }

  private async mountPayment(clientSecret?: string): Promise<void> {
    if (!clientSecret) {
      this.checkoutBusy = false;
      this.checkoutError = 'The payment provider did not return a payment session.';
      return;
    }

    this.api.getStripeConfig().subscribe({
      next: async ({ publishableKey }) => {
        if (!publishableKey || !publishableKey.startsWith('pk_')) {
          this.checkoutBusy = false;
          this.checkoutError = 'Stripe publishable key is not configured.';
          return;
        }
        this.stripe = await loadStripe(publishableKey);
        if (!this.stripe) {
          this.checkoutBusy = false;
          this.checkoutError = 'Stripe could not be initialized.';
          return;
        }
        this.stripeElements = this.stripe.elements({ clientSecret });
        this.paymentElement = this.stripeElements.create('payment');
        this.paymentReady = true;
        this.checkoutBusy = false;
        setTimeout(() => this.paymentElement?.mount('#stripe-payment-element'));
      },
      error: (error: unknown) => {
        this.checkoutBusy = false;
        this.checkoutError = this.errorMessage(error);
      },
    });
  }

  private completeRedirectedPayment(): void {
    const clientSecret = new URLSearchParams(window.location.search).get(
      'payment_intent_client_secret',
    );
    if (!clientSecret) return;

    const pending = sessionStorage.getItem('talabat.pending-checkout');
    if (!pending) {
      this.showToast('Payment returned, but checkout details were not found.', 'error');
      return;
    }

    this.api.getStripeConfig().subscribe({
      next: async ({ publishableKey }) => {
        const stripe = await loadStripe(publishableKey);
        if (!stripe) {
          this.showToast('Payment confirmation could not be checked.', 'error');
          return;
        }
        const result = await stripe.retrievePaymentIntent(clientSecret);
        if (result.paymentIntent?.status === 'succeeded') {
          this.createOrderAfterPayment();
        } else {
          this.showToast('Payment has not completed.', 'error');
        }
      },
      error: (error: unknown) => this.showToast(this.errorMessage(error), 'error'),
    });
  }

  private createOrderAfterPayment(): void {
    const pending = sessionStorage.getItem('talabat.pending-checkout');
    if (!pending) {
      this.checkoutError = 'Checkout details expired. Please contact support before retrying.';
      return;
    }

    const checkout = JSON.parse(pending) as {
      basketId: string;
      deliveryMethodId: number;
      address: ShippingAddress;
    };
    this.checkoutBusy = true;
    this.api
      .createOrder({
        basketId: checkout.basketId,
        deliveryMethodId: checkout.deliveryMethodId,
        shippingAddress: checkout.address,
      })
      .subscribe({
        next: () => {
          this.api.deleteBasket(checkout.basketId).pipe(
            catchError((error: unknown) => {
              if (error instanceof HttpErrorResponse && error.status === 404) {
                return of({ success: true });
              }
              return throwError(() => error);
            }),
          ).subscribe({
            next: () => {
              this.basket = { id: checkout.basketId, items: [] };
              this.checkoutBusy = false;
              this.checkoutOpen = false;
              this.basketOpen = false;
              this.paymentReady = false;
              this.clearStripeElement();
              sessionStorage.removeItem('talabat.pending-checkout');
              this.showToast('Payment confirmed — your order is placed');
              this.view = 'orders';
              this.loadOrders();
              history.replaceState({}, document.title, window.location.pathname);
            },
            error: (error: unknown) => {
              this.checkoutBusy = false;
              this.checkoutError = `Order placed, but the basket could not be cleared: ${this.errorMessage(error)}`;
            },
          });
        },
        error: (error: unknown) => {
          this.checkoutBusy = false;
          this.checkoutError = this.errorMessage(error);
        },
      });
  }

  openAuth(mode: AuthMode = 'login'): void {
    this.authMode = mode;
    this.authOpen = true;
    this.authError = '';
  }

  submitAuth(): void {
    this.authBusy = true;
    this.authError = '';
    const result =
      this.authMode === 'login'
        ? this.api.login(this.authEmail, this.authPassword)
        : this.api.register({
            displayName: this.registerName,
            email: this.authEmail,
            userName: this.registerUsername,
            password: this.authPassword,
            phoneNumber: this.registerPhone,
          });

    result.subscribe({
      next: (token) => {
        localStorage.setItem('talabat.access-token', token.token);
        this.api.getCurrentUser().subscribe({
          next: (user) => {
            this.user = user;
            this.authBusy = false;
            this.authOpen = false;
            this.authPassword = '';
            this.showToast(`Welcome, ${user.displayName}`);
            if (this.view === 'orders') this.loadOrders();
          },
          error: (error: unknown) => {
            this.authBusy = false;
            this.authError = this.errorMessage(error);
            this.logout(false);
          },
        });
      },
      error: (error: unknown) => {
        this.authBusy = false;
        this.authError = this.errorMessage(error);
      },
    });
  }

  logout(showMessage = true): void {
    localStorage.removeItem('talabat.access-token');
    this.user = null;
    this.orders = null;
    if (showMessage) this.showToast('You have signed out');
  }

  setView(view: StoreView): void {
    this.view = view;
    if (view === 'orders') this.loadOrders();
    if (view === 'cache') this.loadCacheKeys();
  }

  loadOrders(): void {
    if (!this.user) {
      this.orders = null;
      this.ordersError = 'Sign in to view your orders.';
      return;
    }
    this.ordersLoading = true;
    this.ordersError = '';
    this.api.getOrders(this.user.email).subscribe({
      next: (summary) => {
        this.orders = summary;
        this.ordersLoading = false;
      },
      error: (error: unknown) => {
        this.orders = null;
        this.ordersLoading = false;
        if (error instanceof HttpErrorResponse && error.status === 404) return;
        this.ordersError = this.errorMessage(error);
      },
    });
  }

  toggleOrder(order: UserOrder): void {
    this.expandedOrderId = this.expandedOrderId === order.id ? null : order.id;
  }

  loadCacheKeys(): void {
    this.cacheLoading = true;
    this.cacheError = '';
    this.api.getCacheKeys().subscribe({
      next: (keys) => {
        this.cacheKeys = keys;
        this.cacheLoading = false;
      },
      error: (error: unknown) => {
        this.cacheError = this.errorMessage(error);
        this.cacheLoading = false;
      },
    });
  }

  imageUrl(url: string): string {
    if (!url) return '';
    try {
      const parsed = new URL(url, window.location.origin);
      const path = parsed.pathname.replace(/^\/images\/products\//i, '/Images/Products/');
      return `${path}${parsed.search}`;
    } catch {
      return url;
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-EG', {
      style: 'currency',
      currency: 'EGP',
    }).format(price);
  }

  openProduct(product: Product): void {
    this.api.getProduct(product.id).subscribe({
      next: (details) => (this.selectedProduct = details),
      error: (error: unknown) => this.showToast(this.errorMessage(error), 'error'),
    });
  }

  closeModals(): void {
    this.authOpen = false;
    this.checkoutOpen = false;
    this.selectedProduct = null;
    this.clearStripeElement();
  }

  private saveBasket(basket: Basket, message?: string): void {
    this.basketBusy = true;
    this.api.saveBasket(basket).subscribe({
      next: (saved) => {
        this.basket = saved;
        this.basketBusy = false;
        if (message) this.showToast(message);
      },
      error: (error: unknown) => {
        this.basketBusy = false;
        this.showToast(this.errorMessage(error), 'error');
      },
    });
  }

  private loadBasket(): void {
    this.api
      .getBasket(this.basket.id)
      .pipe(
        catchError((error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 404) return of(null);
          return throwError(() => error);
        }),
      )
      .subscribe({
        next: (basket) => {
          if (basket) this.basket = basket;
        },
        error: (error: unknown) => this.showToast(this.errorMessage(error), 'error'),
      });
  }

  private getBasketId(): string {
    const stored = localStorage.getItem('talabat.basket-id');
    if (stored) return stored;
    const id = crypto.randomUUID();
    localStorage.setItem('talabat.basket-id', id);
    return id;
  }

  private toBasketItem(product: Product): BasketItem {
    return {
      id: product.id,
      productName: product.name,
      price: product.price,
      quantity: 1,
      pictureUrl: product.pictureUrl,
      brand: product.productBrand,
      type: product.productType,
    };
  }

  private isAddressValid(): boolean {
    return Object.values(this.shippingAddress).every((value) => value.trim().length > 0);
  }

  private isAddressEmpty(): boolean {
    return Object.values(this.shippingAddress).every((value) => !value.trim());
  }

  private emptyAddress(): ShippingAddress {
    return { firstName: '', lastName: '', street: '', city: '', state: '' };
  }

  private clearStripeElement(): void {
    this.paymentElement?.destroy();
    this.paymentElement = null;
    this.stripeElements = null;
    this.stripe = null;
    this.paymentReady = false;
  }

  private errorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as
        | { message?: string; title?: string; errors?: string[] }
        | string
        | null;
      if (typeof body === 'string' && body) return body;
      if (body && typeof body === 'object') {
        if (body.message) return body.message;
        if (body.title) return body.title;
        if (body.errors?.length) return body.errors.join(' ');
      }
      if (error.status === 0) return 'Could not reach the API. Check that the backend is running.';
      return `Request failed (${error.status}).`;
    }
    return error instanceof Error ? error.message : 'Something went wrong. Please try again.';
  }

  private showToast(message: string, kind: 'success' | 'error' = 'success'): void {
    this.toastMessage = message;
    this.toastKind = kind;
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => (this.toastMessage = ''), 4200);
  }
}
