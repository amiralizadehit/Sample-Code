package com.arsheet.arsheetpresencesystem;

import java.nio.Buffer;
import java.sql.Date;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;



import android.annotation.SuppressLint;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Fragment;
import android.app.FragmentManager;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.database.Cursor;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.text.format.DateFormat;
import android.text.style.BackgroundColorSpan;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Adapter;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.GridView;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;


@SuppressLint("NewApi") public class MainActivity extends Activity implements OnItemSelectedListener {
	public static MainActivity globalActivity = null;
	public static DatabaseHelper db;
	public static Context globalContext;
	public static FragmentManager fragmentManager;
	public static Resources res; 
	public static TextView passwordField;
	public static TextView usernameField;
	public static TextView time;
	public static Calendar calendar;
    public static SimpleDateFormat timeFormat;
    public static RelativeLayout rLayout;
	
    
    
    private Spinner spinner;
    private TextView type;
    private Button timeRefresher;
    private Button admin;
    private Button record;
    private SimpleDateFormat dateFormat;
    
    
    

    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
         globalContext = this.getApplicationContext();
         globalActivity = this;
         fragmentManager = getFragmentManager();
         res = getResources(); //resource handle
        
        passwordField = (TextView)findViewById(R.id.passwordText);
        usernameField = (TextView)findViewById(R.id.usernameText);
        
        db = new DatabaseHelper(this);
        
        
        
        
        spinner=(Spinner) findViewById(R.id.spinner1);
        admin = (Button)findViewById(R.id.adminpage);
        record=(Button) findViewById(R.id.button1);
        time=(TextView) findViewById(R.id.textView1);
        timeFormat = new SimpleDateFormat("HH:mm:ss a");
        dateFormat = new SimpleDateFormat("dd");
        calendar = Calendar.getInstance();
        
        time.setText(timeFormat.format(calendar.getTime()));
        rLayout = (RelativeLayout) findViewById (R.id.relative);

        
        
       
    
        ArrayAdapter adapter=ArrayAdapter.createFromResource(this,R.array.outworks, R.layout.custom_xml_spinner_layout);
        spinner.setAdapter(adapter);
        spinner.setOnItemSelectedListener(this);
        
        
    }
    
    public void onItemSelected(AdapterView<?> adapterView, View view, int i ,long l) {
		type = (TextView)view;
	}
    
    public void Admin (View view){
    	String enteredUsername = usernameField.getText().toString();
    	String enteredPassword = passwordField.getText().toString();
    	
    	int rank =db.AdminPage(enteredUsername, enteredPassword);
    	if (rank ==2){
    		// Starting admin activity
    		Toast.makeText(getApplicationContext(), "You are admin!",Toast.LENGTH_LONG).show();
    		AdminPage();
    		}
    	else if (rank ==1){
    		AlertDialog.Builder builder = new AlertDialog.Builder(this);
        	builder.setCancelable(true);
        	builder.setTitle("Error");
        	builder.setMessage("You are not allowded to access admin page");
        	builder.show(); 
    	}
    	else
    		Toast.makeText(getApplicationContext(), "Username or Password is incorrect",Toast.LENGTH_LONG).show();
    }
    public void AdminPage() {
    	Intent i = new Intent(this,Admin_page.class);
		startActivity(i); //Declaring new activity using Intents
	}

    public void Record(View view){
    	
    	calendar = Calendar.getInstance();
    	 
    	String enteredUsername = usernameField.getText().toString();
    	String enteredPassword = passwordField.getText().toString();
    	int rank =db.InformationProcessor(enteredUsername, 
    			enteredPassword ,type.getText().toString(),dateFormat.format(calendar.getTime()),timeFormat.format(calendar.getTime()));
    	if (rank>=1){
    		BackgroundChanger (true);
    	}
    	}

    
    public static void Reset (){
    	passwordField.setText("");
    	usernameField.setText("");
    	calendar = Calendar.getInstance();
    	time.setText(timeFormat.format(calendar.getTime()));
    	BackgroundChanger(false);
    	
  
    	
    }
    
    public static void BackgroundChanger (boolean correction){
    	if(correction==false){ 
    	Drawable drawable = res.getDrawable(R.drawable.pig1); //new Image that was added to the res folder
    	rLayout.setBackground(drawable);}
    	else{
        Drawable drawable = res.getDrawable(R.drawable.pass); //new Image that was added to the res folder
        rLayout.setBackground(drawable);}
    	}




    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();
        if (id == R.id.action_settings) {
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

	@Override
	public void onNothingSelected(AdapterView<?> arg0) {
		// TODO Auto-generated method stub

	}
	}

