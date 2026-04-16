namespace Dreamteck.Splines.Editor
{
	public class ComputerEditorModule : EditorModule
	{
		protected SplineComputer m_spline;
		public EmptySplineHandler repaintHandler;
		public SplineEditorBase.UndoHandler undoHandler;

		public ComputerEditorModule(SplineComputer spline)
		{
			m_spline = spline;
		}

		protected override void RecordUndo(string title)
		{
			base.RecordUndo(title);
			if (undoHandler != null)
			{
				undoHandler(title);
			}
		}

		protected override void Repaint()
		{
			base.Repaint();
			if (repaintHandler != null)
			{
				repaintHandler();
			}
		}
	}
}