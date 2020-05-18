using BillingManagement.Business;
using BillingManagement.Models;
using BillingManagement.UI.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace BillingManagement.UI.ViewModels
{
    class MainViewModel : BaseViewModel
    {
		private BaseViewModel _vm;

		public BaseViewModel VM
		{
			get { return _vm; }
			set {
				_vm = value;
				OnPropertyChanged();
			}
		}

		private string searchCriteria;

		public string SearchCriteria
		{
			get { return searchCriteria; }
			set { 
				searchCriteria = value;
				OnPropertyChanged();
			}
		}

		private BillingManagementContext db;
		
		public BillingManagementContext Db
		{
			get => db;
			set
			{
				db = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<Customer> _customers;

		public ObservableCollection<Customer> Customers
		{
			get => _customers;
			set
			{
				_customers = value;
				OnPropertyChanged();
			}
		}

		CustomerViewModel customerViewModel;
		InvoiceViewModel invoiceViewModel;

		public ChangeViewCommand ChangeViewCommand { get; set; }

		public DelegateCommand<object> AddNewItemCommand { get; private set; }

		public DelegateCommand<Invoice> DisplayInvoiceCommand { get; private set; }
		public DelegateCommand<Customer> DisplayCustomerCommand { get; private set; }

		public DelegateCommand<Customer> AddInvoiceToCustomerCommand { get; private set; }

		public RelayCommand<Customer> SearchCommand { get; private set; }


		public MainViewModel()
		{

			db = new BillingManagementContext();
			setDB();

			Customers = new ObservableCollection<Customer>();

			ChangeViewCommand = new ChangeViewCommand(ChangeView);
			DisplayInvoiceCommand = new DelegateCommand<Invoice>(DisplayInvoice);
			DisplayCustomerCommand = new DelegateCommand<Customer>(DisplayCustomer);

			AddNewItemCommand = new DelegateCommand<object>(AddNewItem, CanAddNewItem);
			AddInvoiceToCustomerCommand = new DelegateCommand<Customer>(AddInvoiceToCustomer);

			SearchCommand = new RelayCommand<Customer>(SearchContact, CanSearchContact);

			customerViewModel = new CustomerViewModel();
			invoiceViewModel = new InvoiceViewModel(customerViewModel.Customers);

			VM = customerViewModel;

		}

		private void setDB()
		{
			if (db.Customers.Count() == 0)
			{
				List<Customer> customers = new CustomersDataService().GetAll().ToList();
				List<Invoice> invoices = new InvoicesDataService(customers).GetAll().ToList();

				foreach (Customer c in customers)
					db.Customers.Add(c);

				db.SaveChanges();
			}
		}

		private void ChangeView(string vm)
		{
			switch (vm)
			{
				case "customers":
					VM = customerViewModel;
					break;
				case "invoices":
					VM = invoiceViewModel;
					break;
			}
		}

		private void DisplayInvoice(Invoice invoice)
		{
			invoiceViewModel.SelectedInvoice = invoice;
			VM = invoiceViewModel;
		}

		private void DisplayCustomer(Customer customer)
		{
			customerViewModel.SelectedCustomer = customer;
			VM = customerViewModel;
		}

		private void AddInvoiceToCustomer(Customer c)
		{
			var invoice = new Invoice(c);
			c.Invoices.Add(invoice);
			DisplayInvoice(invoice);
		}

		private void AddNewItem (object item)
		{
			if (VM == customerViewModel)
			{
				var c = new Customer();
				customerViewModel.Customers.Add(c);
				customerViewModel.SelectedCustomer = c;
			}
		}

		private bool CanAddNewItem(object o)
		{
			bool result = false;

			result = VM == customerViewModel;
			return result;
		}

		private void SearchContact(object parameter)
		{

			List<Customer> customers = new List<Customer>();
			string input = searchCriteria as string;
			int output;
			string searchMethod;

			if (!Int32.TryParse(input, out output))
			{
				searchMethod = "name";
			}
			else
			{
				searchMethod = "id";
			}

			switch (searchMethod)
			{
				case "id":

					customerViewModel.SelectedCustomer = db.Customers.Find(output);
					break;

				case "name":

					customers = db.Customers.Where(c => (c.LastName.ToLower().StartsWith(input)) || (c.Name.ToLower().StartsWith(input))).ToList();
					Customers.Clear();

					if (customers.Count > 0)
					{

						foreach (Customer c in customers)
							Customers.Add(c);

						Customers = new ObservableCollection<Customer>(Customers.OrderBy(c => c.LastName));

						customerViewModel.ShowSearchResult(Customers);

					} else if(customers.Count == 1)
					{

						customerViewModel.SelectedCustomer = db.Customers.Find(customers);

					}

					break;
				default:
					MessageBox.Show("Unkonwn search method");
					break;

			}
		}

		private bool CanSearchContact(object c)
		{
			if (VM == customerViewModel) return true;
			else return false;
		}

	}
}
