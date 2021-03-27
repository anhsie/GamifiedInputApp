using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;

namespace GamifiedInputApp
{
    public class ContentHelper
    {
        MainWindow m_mainWinodw;
        Compositor m_compositor;
        ExpCompositionContent m_content;
        ExpInputSite m_inputSite;
        SpriteVisual m_backgroundVisual;

        public ContentHelper(MainWindow mainWindow)
        {
            m_mainWinodw = mainWindow;
            m_compositor = m_mainWinodw.Compositor;
            m_content = ExpCompositionContent.Create(m_compositor);
            m_content.AppData = this;

            ScalingRect rect = m_mainWinodw.GameBounds;

            m_backgroundVisual = m_compositor.CreateSpriteVisual();
            m_backgroundVisual.Brush = m_compositor.CreateColorBrush(Microsoft.UI.Colors.White);
            m_backgroundVisual.Offset = new System.Numerics.Vector3((float)rect.Left, (float)rect.Top, 0.0f);
            m_backgroundVisual.Size = new System.Numerics.Vector2((float)rect.ActualWidth, (float)rect.ActualHeight);
            m_backgroundVisual.Scale = new System.Numerics.Vector3((float)rect.ScaleX, (float)rect.ScaleY, 1.0f);
            m_content.Root = m_backgroundVisual;

            m_inputSite = ExpInputSite.GetOrCreateForContent(m_content);

            m_mainWinodw.SizeChanged += M_mainWindow_SizeChanged;
        }

        public void Dispose()
        {
            m_backgroundVisual.Dispose();
            m_backgroundVisual = null;
            m_mainWinodw.SizeChanged -= M_mainWindow_SizeChanged;
        }

        private void M_mainWindow_SizeChanged(object sender, object args)
        {
            ScalingRect rect = m_mainWinodw.GameBounds;
            m_backgroundVisual.Offset = new System.Numerics.Vector3((float)rect.Left, (float)rect.Top, 0.0f);
            m_backgroundVisual.Scale = new System.Numerics.Vector3((float)rect.ScaleX, (float)rect.ScaleY, 1.0f);
        }

        public ExpCompositionContent Content
        {
            get { return m_content; }
        }

        public ExpInputSite InputSite
        {
            get { return m_inputSite; }
        }

        public SpriteVisual RootVisual
        {
            get { return m_backgroundVisual; }
        }
    }
}
