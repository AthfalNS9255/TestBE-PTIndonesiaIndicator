package com.siadminmovie.service;

import java.util.List;

import org.springframework.data.domain.Page;

import com.siadminmovie.model.Movie;

public interface MovieService {
    List<Movie> getAllMovies();
	void saveMovie(Movie movie);
	Movie getMovieById(int id);
	void deleteMovieById(int id);
	Page<Movie> findMovies(int pageNo, int pageSize, String sortField, 
		String sortDirection, String keyword);
}
