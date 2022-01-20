/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package bridgescargo;

import javax.swing.*;
import javafx.application.Platform;
import javafx.embed.swing.JFXPanel;
import javafx.scene.*;
import javafx.scene.web.WebEngine;
import javafx.scene.web.WebView;

public class googlemap extends JFrame {

    private WebEngine webEngine;
    private JFXPanel panel;
    
    public googlemap() {
        setTitle("Search");
        setVisible(true);
        setBounds(0,0,1950, 1080);
        
        panel = new JFXPanel();
        add(panel);
        
        Platform.runLater(() -> {
            WebView view = new WebView();
            view.getEngine().load("file:///C:/Users/maske/OneDrive/Masa%C3%BCst%C3%BC/simple_map.html");

            panel.setScene(new Scene(view));
        });
        
       
    }

    

}
