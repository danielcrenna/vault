# Paging
## Bulletproof paging for queryables and enumerables

### Introduction
Working with paging is frustrating. You always have to roll your own solution,
and manually interpret the results of query parameters to decide how to pull 
data into views. Thankfully most decent data access strategies have support for 
IQueryable, which we can take advantage of in data access repositories: you provide 
an optional page and count, and the queryable knows how to get the slice of data 
you want in the most efficient way. This library fills in all the time-consuming
details, so you can just ship your paging views.

### Features

* Complete - Handles all major paging scenarios easily
* Efficient - Evaluates the inner queryable exactly once, and only when needed
* Intuitive - Use a simple convention to stop worrying about paging
* Helpful - Use Razor helpers to make building UI a breeze

### Usage

In a typical repository interface you'll define methods to return results.
Here's an example:

	public interface IUnicornRepository
	{
		IEnumerable<Unicorn> All();    
	}

That All() method is naturally designed to fetch the entire data set in your
repository implementation. What if there are fifty million unicorns? You've
probably had this problem before, and the solution is to "slice" your data
into pages. Assuming you've chosen a nice data access technology that will
let you build up IQueryable expressions that are evaluated only when they
are accessed, using this library means you're pretty much done. Here's how
to define a pageable version of the above:

	using Paging;

	public interface IUnicornRepository
	{
		IPagedEnumerable<Unicorn> All(int? page, int? count);
	}

We've replaced the regular IEnumerable iterable with one that supports
paging, and provided two nullable parameters to define the actual data
slice. The parameters can come from anywhere. We'll talk about that a little
later, but here's a typical ASP.NET MVC controller where you're passing these
values from the query string down to the repository:

	using System.Web.Mvc;
	using Paging;

	public class UnicornView
	{
		IPagedEnumerable<Unicorn> Unicorns { get; set; }
	}
	
	public class UnicornController : Controller
	{
		private readonly IUnicornRepository _repository;

		public UnicornController(IUnicornRepository repository)
		{
			_repository = repository;
		}

		public ActionResult Index(int? page, int? count)
        {
			var unicorns = _repository.All(page, count);

            var model = new UnicornView() { Unicorns = unicorns };

            return View(model);
        }
	}

So far so good, right? All that's left is the implementation. This library
only performs the wrapping, so all you need to do is do what you would normally
do to get your regular All() result as an IQueryable, and then wrap it in a
PagedQueryable<T>:
	
    using Paging;
    using YourDataAccessProvider;

    public UnicornRepository : IUnicornRepository
    {
        public IPagedEnumerable<Unicorn> All(int? page, int? count)
        {
            IQueryable<Unicorn> unicorns = YourDataAccessProvider.UnitOfWork.Current.Unicorns;
           
            return new PagedQueryable<Unicorn>(unicorns, page, count);
        }
    }		

Once a PagedQueryable is evaluated, it won't be evaluated again, so you can
be sure that your data access is only being exercised exactly when it should
(when you're crossing the repository wall through an IEnumerable interface).
That's what IPagedEnumerable is for; so you don't leak queryables like a 
pottery animal full of holes.

_Side note_: Counting a queryable source should never cause the backing store to fetch 
all those objects, and if we do want to get the count, we only want to do it
once!

### Using paging in UI views

Now that you know how to wrap your queryables into sliceable page versions, it's time
to handle when your controller passes view models with IPagedEnumerable<T> instances
in it, like the controller example above. IPagedEnumerable<T> also inherits from a 
thinner interface called IPageable. IPageable is agnostic about the data underneath it, 
so it's perfect for constructing pager views.

If you're using ASP.NET MVC 3, here is how you would build Razor helper methods
to simplify building complex pager views. In your model, you'd specify that some 
collection property is an IPagedEnumerable, so you could access it in your view:

	using Paging;

	public class UnicornView
	{
		IPagedEnumerable<Unicorn> Unicorns { get; set; }
	}

From there, these helpers are great to toss into your App_Code folder and use anywhere
you need to display a pager for sliced data in your view model (remember to reference the
Paging library if it's compiled, or pull in the source code):

	@using Paging;

	@helper PagerHeader(IPageable source, string className="") {
	<div class="@className">
		<p>Showing <b>@source.PageStart - @source.PageEnd</b> of <b>@source.TotalCount</b> results</p>
	</div>
	}

	@helper PagerPrev(IPageable source, string text = "prev") {
		var prev = source.PageIndex - 1;
		if (source.HasPreviousPage)
		{
			<a href="?page=@prev">@text</a>
		}  
	}

	@helper PagerNext(IPageable source, string text = "next") {
		var next = source.PageIndex + 1;
		if (source.HasNextPage)
		{
			<a href="?page=@next">@text</a>
		}    
	}

	@helper Pager(IPageable source, string className = "") {   
		var last1 = source.TotalPages;
		var last2 = source.TotalPages - 1;
		var delta = Math.Abs(source.TotalPages - source.PageIndex);               
		var start = delta > 8 ? source.PageIndex > 3 ? source.PageIndex - 2 : 1 : source.PageIndex > 1 ? source.PageIndex - 2 : 1;
		var cells = delta > 8 ? source.PageIndex >= 3 ? source.PageIndex + 2 : 4 : source.PageIndex >= 7 ? source.PageIndex + 1 : 7;
	<div class="@className">
		<p>	    
			@*Display the previous page link if available*@
			@PagerPrev(source)

			@*Display first page and ellipsis if index is far enough away*@
			@if (source.PageIndex > 3) {            
			<a href="?page=1">1</a>
				if(source.PageIndex > 4) {                        
			<text>...</text>               
				}         
			}
        
			@*Display pager cells*@        
			@for (var i = start > 0 ? start : 1; i < cells; i++) {
				if (source.PageIndex == i) {
			<span class="pagination-active">@i</span>
				} else {
			<a href="?page=@i">@i</a>
				}
			}

			@*Display last two pages and ellipsis if index is far enough away*@
			@if (source.TotalPages > 8 && source.PageIndex < last2 - 1) {        
			<text>...</text>           
			}
        
			@*Display the last two page cells if we haven't already*@
			@if (source.PageIndex <= last1 - 1) {
				if(source.PageIndex != last2) {
			<a href="?page=@last2">@last2</a>
				}
			<a href="?page=@last1">@last1</a>
			}
        
			@*Display the next page link if available*@
			@PagerNext(source)
		</p>
	</div>
	}

The pager helpers above will help you construct a pager that is easy to use for large
or small page sets. You can always modify the code to fit your own UI needs. Here is
how you might display a pager using helpers in your own view:

    @model UnicornView;
    ...
    <div">
        @_Helpers.PagerHeader(Model.Unicorns, "pager-header-class")
        @_Helpers.Pager(Model.Unicorns, "pager-class")          
    </div>
    ...

With that code, and some CSS, you'd get something like this:

![pager](http://apitize.com.s3.amazonaws.com/pager.png)

You'll notice that IPageable has properties PageStart, PageEnd, and PageCount to 
help determine the index of the items in your data slice and the actual number of items it 
currently contains (we all know the last page rarely has as many items as you asked for). 
This is useful beyond the usual IPageable suspects of PageIndex, PageSize, and TotalPages
for building detailed pagers.

### Accepting page and count values

A view doesn't have to be explicit with the count value; it is either passed in 
on the server side by you when wrapping your IQueryable, or the PagedQueryable.DefaultPageSize
is used if one is not provided. Similarly, if a page value is not provided, the
first page is selected, so you can always create an instance of PagedQueryable with whatever
information your controller has at the time. In ASP.NET MVC, query string values are
automatically bound at runtime, so using _page_ and _count_ in the query string automatically
maps to the values you want.

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // ...

            // You can globally set your page size
            PagedQueryable.DefaultPageSize = 30;

            // ...
        }
    }

    using Paging;
    using YourDataAccessProvider;

    // This version ignores a passed in count and sets an explicit one
    public UnicornRepository : IUnicornRepository
    {
        public IPagedEnumerable<Unicorn> All(int? page)
        {
            IQueryable<Unicorn> unicorns = YourDataAccessProvider.UnitOfWork.Current.Unicorns;
    
            // Wrap your data access provider's queryable in a sexy paging blanket
            return new PagedQueryable<Unicorn>(unicorns, page, 20 /* count */);
        }
    }  
    
### One last thing

Sometimes you want to pass back an empty collection to a paging method. For that you can 
use the PagedEnumerable instance itself, and give it an empty collection.

    using Paging;
    using YourDataAccessProvider;
    using System.Linq;

    public UnicornRepository : IUnicornRepository
    {
        public IPagedEnumerable<Unicorn> All(int mana, int? page, int? count)
        {
            if(mana > 100)
            {
                var unicorns = YourDataAccessProvider.UnitOfWork.Current.Unicorns;
                return new PagedQueryable<Unicorn>(unicorns, page, count);
            }
            else
            {
                // Unicorns are only visible to those magic enough to see them
                var unicorns = Enumerables.Empty<Unicorn>();
                return new PagedEnumerable<Unicorn>(unicorns, page, count);
            }           
        }
    }    

That's it. Paging, solved.