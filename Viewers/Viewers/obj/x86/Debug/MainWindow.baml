   M S B A M L   `   `   `  ����B  >Viewers, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:Viewers.UserControlsViewers.UserControls  5   	   J FGestureControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\?clr-namespace:GestureControls.Controls;assembly=GestureControlsGestureControls.Controls 5   	   !clr-namespace:ViewersViewers  5   	   ^ ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers 5   Z       Viewers.MainWindow   5   	   *my1"clr-namespace:Viewers.UserControls  W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Pctrl?clr-namespace:GestureControls.Controls;assembly=GestureControls     5   	   #cclr-namespace:Viewers     5   	   mmy^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers     5   Z   -   5          � Title$  
MainWindow��5   	   
���   � � WindowState$ 	Maximized=�6/    � WindowStyle$
 None=�6G   .��5
      �� -   5        $��ImLayout��5
      
���   �.+�5   
   =  7Microsoft.Samples.Kinect.WpfViewers.KinectSensorChooser  -   5        $��_KinectSensor1��5   !    K� ZIndex$ 0��67   $	��600��6H   $
��1024��6U   $��Center=�6b   $��Center=�6   �� 5   
   -   5        $��ImBackGround��5      $ 0��6$   $$��Pictures/ImView_BackGround.png��65   >  8Microsoft.Samples.Kinect.WpfViewers.KinectSkeletonViewer  5   
   -   5        $��_SkeleViewer��5   "   $ 1��66   5  /Microsoft.Samples.Kinect.WpfViewers.ImageViewer   Kinect 6G   ��  �� ElementName$ _KinectSensor1�� �� Path$ Kinect��� 5   
   -   5        $��TxtInformation��5      $ 1��6*   
���OO/� J� Opacity$	 0.8Q�6V   $��26(�6d   $��Century)�6r    �� TextAlignment$
 Left=�6�   �� 5   
   -   5        $��TxtTilt��5      $ 1��6#   
���OO/�$��26(�6O   $��Century)�6]   $ Center=�6r   +  %GestureControls.Controls.MagnetButton  5   
   -   5        $��Bttn_DownTilt��5      $ 1��61   $��24(�6S   $��-��6a   $��	ExtraBold"�6m     5   
   -	   5        $��Bttn_UpTilt��5      $ 1��6/   $��24(�6O   $��+��6]   $��	ExtraBold"�6i   �� 5   
   -
   5        $��_Folder��5      $ 1��6   *  $GestureControls.Controls.HoverButton  5   
   -   5        $��Bttn_Browser��5      $��Change Fldr��6/   $ 2��6E   6V   r� 5   
   -   5        $��_VideoPlayer��5      $ 0��6+   �� 5   
   -   5        $��	ImContent��5      5      5      