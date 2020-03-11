using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery.Part
{
	public class PagingLayout : WebControl, INamingContainer
	{
		public class PagingContainer : Control, INamingContainer
		{
			public long PageNumber { get; set; }
			public long PageIndex { get; set; }

			public string ParsePageNumber(int minDigits)
			{
				return PageNumber.ToString().PadLeft(minDigits, '0');
			}

			public PagingContainer(long pageNumber)
			{
				PageNumber = pageNumber;
				PageIndex = pageNumber - 1;
			}
		}

		const int sideLength = 2;

		#region Fields
		bool m_isRTL;
		int m_shownPagesNumber;
		int m_middleLength; // (edge x 2) + ((...) x 2)
		PlaceHolder m_result;
		int m_activePageNumber;
		long m_pagesCount;
		#endregion

		#region Properties
		public string PlaceHolderID { get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(PagingContainer))]
		public ITemplate PageButtonTemplate { get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer(typeof(PagingContainer))]
		public ITemplate ActivePageButtonTemplate { get; set; }

		[PersistenceMode(PersistenceMode.InnerProperty)]
		public ITemplate SeperatorTemplate { get; set; }


		public int ShownPagesNumber
		{
			get
			{
				return m_shownPagesNumber;
			}
			set
			{
				if (value >= 3 && ((value % 2) == 1))
				{
					m_shownPagesNumber = value;
				}
				else
				{
					throw new Exception("Problem");
				}
			}
		}
		#endregion

		#region Ctor
		public PagingLayout()
		{
			ShownPagesNumber = 7;
		}
		#endregion

		#region Public methods
		public Control Generate(int activePageIndex, long pagesCount, bool isRTL)
		{
			// initialize
			m_result = new PlaceHolder();
			m_isRTL = isRTL;
			m_activePageNumber = activePageIndex + 1;
			m_pagesCount = pagesCount;
			m_middleLength = ShownPagesNumber - (2 * sideLength);
			int halfMiddleLength = (m_middleLength - 1) / 2;


			if (m_pagesCount <= ShownPagesNumber)
			{
				// show according to pages count
				for (int i = 1; i <= m_pagesCount; i++)
				{
					addButton(i);
				}
			}
			else
			{
				if (ShownPagesNumber == 3)
				{
					if (activePageIndex == 0)
					{
						// first page is choosen
						for (int i = 1; i <= 3; i++)
						{
							addButton(i);
						}
						//addSeperator();
					}
					else if (activePageIndex + 1 == m_pagesCount)
					{
						//addSeperator();
						// last page is choosen
						for (int i = (int) m_pagesCount - 3; i < m_pagesCount; i++)
						{
							addButton(i);
						}
					}
					else
					{
						//addSeperator();
						for (int i = 0; i < 3; i++)
						{
							addButton(activePageIndex + i);
						}
						//addSeperator();
					}
				}
				else if (m_pagesCount == ShownPagesNumber + 1)
				{
					if ((m_pagesCount / 2 - m_activePageNumber) >= 1)
					{
						startLayout();
					}
					else
					{
						endLayout();
					}
				}
				else // pc.PagesCount >= availablePageSpace + 2
				{
					if ((halfMiddleLength + sideLength) > m_activePageNumber)
					{
						startLayout();
					}
					else if (m_pagesCount - (halfMiddleLength + sideLength) < m_activePageNumber)
					{
						endLayout();
					}
					else
					{
						middleLayout();
					}
				}
			}
			return m_result;
		}
		#endregion

		#region Private methods
		private void middleLayout()
		{
			addButton(1);
			addSeperator();

			int pageNumber = m_activePageNumber - ((m_middleLength - 1) / 2);

			for (int i = 1; i <= m_middleLength; i++)
			{
				addButton(pageNumber);
				pageNumber++;
			}

			addSeperator();
			addButton(m_pagesCount);
		}

		private void endLayout()
		{
			addButton(1);
			addSeperator();

			for (long i = (m_pagesCount - (sideLength + m_middleLength)); i <= m_pagesCount; i++)
			{
				addButton(i);
			}
		}

		private void startLayout()
		{
			for (int i = 1; i <= (sideLength + m_middleLength); i++)
			{
				addButton(i);
			}

			addSeperator();
			addButton(m_pagesCount);
		}

		private void addSeperator()
		{
			Control control = new Control();
			SeperatorTemplate.InstantiateIn(control);

			if (m_isRTL)
			{
				m_result.Controls.AddAt(0, control);
			}
			else
			{
				m_result.Controls.Add(control);
			}
		}

		private void addButton(long pageNumber)
		{
			PagingContainer container = new PagingContainer(pageNumber);

			if (pageNumber == m_activePageNumber)
			{
				ActivePageButtonTemplate.InstantiateIn(container);
			}
			else
			{
				PageButtonTemplate.InstantiateIn(container);
			}

			if (m_isRTL)
			{
				m_result.Controls.AddAt(0, container);
			}
			else
			{
				m_result.Controls.Add(container);
			}
		}
		#endregion
	}
}
