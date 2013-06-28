RenderLib for Windows 8.1 (Preview)
-----

## Intro
Windows 8.1 provides the ability to progressively render templates inside a ListView or GridView control.
This means that you can load important information (such as the Title) before less information (like the background image).

See the [Dramatically Increase Performance when Users Interact with Large Amounts of Data in GridView and ListView](http://channel9.msdn.com/Events/Build/2013/3-158) for more details.

The code demonstrated in the TechEd talk was very verbose and preventing the use of the MVVM pattern. Admittedly, since this is for performance optimisation, MVVM might not be a very good fit.

This library attempts to make usage of this feature much simpler reducing the cost of entry.

## Usage

To use, two properties need to be set:
 - Render.UsePhases="True" on the GridView / ListView
 - Render.Phase="[numeric phase number]"

0 means render initially, anything greater will be rendered as part of a future phase.

Gaps are allowed between phase numbers - i.e. Phases 0, 3, 5 is legal.

Sample usage (see the GroupDetailPage inside the sample for a working example):

    <GridView ... RenderLib:Render.UsePhases="True">
            <GridView.ItemTemplate>
                <DataTemplate>
                    <Grid Height="110" Width="480" Margin="10">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Border Background="Red" Width="110" Height="110" />
                        <Border Background="{StaticResource ListViewItemPlaceholderBackgroundThemeBrush}" Width="110" Height="110" RenderLib:Render.Phase="4">
                            <Image Source="{Binding ImagePath}" Stretch="UniformToFill" AutomationProperties.Name="{Binding Title}"/>
                        </Border>
                        <StackPanel Grid.Column="1" VerticalAlignment="Top" Margin="10,0,0,0"  RenderLib:Render.Phase="1">
                            <Image Width="50" Height="50" Source="{Binding ImagePath}" Stretch="UniformToFill" AutomationProperties.Name="{Binding Title}"  RenderLib:Render.Phase="5" />
                            <TextBlock Text="{Binding Title}" Style="{StaticResource TitleTextBlockStyle}" TextWrapping="NoWrap"/>
                            <TextBlock RenderLib:Render.Phase="2" Text="{Binding Subtitle}" Style="{StaticResource CaptionTextBlockStyle}" TextWrapping="NoWrap" />
                            <TextBlock RenderLib:Render.Phase="3" Text="{Binding Description}" Style="{StaticResource BodyTextBlockStyle}" MaxHeight="60"/>
                        </StackPanel>
                    </Grid>
                </DataTemplate>
            </GridView.ItemTemplate>
    </GridView>

Enjoy!
